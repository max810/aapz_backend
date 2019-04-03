using BLL.Models;
using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BLL
{
    public class StreamingLogic
    {
        public static bool SaverLaunched = false;
        public ConcurrentDictionary<string, DriverVideoStream> VideoStreams { get; private set; } = new ConcurrentDictionary<string, DriverVideoStream>(); // use this

        private AAPZ_BackendContext _context;
        private IConnectionManagerThreadSafe<string> _connManager;
        private HttpClient _httpClient = new HttpClient();
        private ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        private readonly Queue<int> freePorts = new Queue<int>(Enumerable.Range(5000, 65535 - 5000));

        public StreamingLogic(AAPZ_BackendContext context, IConnectionManagerThreadSafe<string> connManager)
        {
            _context = context;
            _connManager = connManager;
        }

        public bool TryGetStream(Driver driver, out DriverVideoStream videoStream)
        {
            return VideoStreams.TryGetValue(driver.IdentifierHashB64, out videoStream);
        }

        public bool StreamExists(Driver driver)
        {
            return _connManager.IsAlive(driver.IdentifierHashB64);
        }

        public int LaunchNewListeningStream(Driver driver, TimeSpan listeningTimeout)
        {
            int nextFreePort = BindNextFreePort();
            TaskHelper.RunBgLong(() => LaunchListeningStream(driver, nextFreePort, listeningTimeout));

            return nextFreePort;
        }

        private int BindNextFreePort()
        {
            return freePorts.Dequeue();
        }

        private async void LaunchListeningStream(Driver driver, int port, TimeSpan listeningTimeout)
        {
            if (!SaverLaunched)
            {
                await LaunchSaver();
                SaverLaunched = true;
            }
            int inferenceEveryFrames = 10;
            double increaseBy = inferenceEveryFrames / 30.0;
            double[] classes = new double[10];
            //AAPZ_BackendContext _context = new AAPZ_BackendContext(); //(AAPZ_BackendContext)_provider.GetService(typeof(AAPZ_BackendContext));
            UdpClient udpClient = new UdpClient(5005);
            bool connectionAlive = false;
            byte[] dgram = new byte[] { };
            string senderAddr = "-";
            Ride ride = new Ride
            {
                Driver = driver,
                DriverId = driver.UserId
            };
            _context.Add(ride);
            _context.SaveChanges();
            Console.WriteLine("Starting receiving stream");
            try
            {
                int i = 0;
                while (true)
                {
                    Task frameAwaiter = Task.Delay(TimeSpan.FromSeconds(30));
                    Task frameReader = Task.Run(() =>
                    {
                        //IPEndPoint
                        dgram = udpClient.Receive(ref carAddr);
                    });

                    await Task.WhenAny(frameReader, frameAwaiter);

                    if (!frameReader.IsCompleted && frameAwaiter.IsCompleted)
                    {
                        // handle not receiving frames
                        //await Clients.Caller.InferenceMessage("-1");
                        _connManager.SetZombie(senderAddr);
                        return;
                    }
                    if (!connectionAlive) // used once just to set true in connections
                    {
                        connectionAlive = true;
                        senderAddr = ipaddr + ":" + port;
                        _connManager.SetAlive(senderAddr);
                        ride.StartTime = DateTime.Now;
                    }
                    _connManager.SetFrame(senderAddr, dgram);
                    //await writer.WriteAsync(dgram);

                    if (i % inferenceEveryFrames == 0)
                    {
                        i = 0;
                        int classLabelId = int.Parse(await MakeInferenceRequest(dgram));
                        classes[classLabelId] += increaseBy;
                        if (classes[classLabelId] >= 1)
                        {
                            classes[classLabelId] -= 1;
                            ride.IncrementDrivingClass(classLabelId);
                        }
                        _connManager.SetClassId(senderAddr, classLabelId);
                        _context.Entry(ride).State = EntityState.Modified;
                    }
                    ++i;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION: " + e.Message);
                throw e;
            }
            finally
            {
                udpClient.Close();
                ride.InProgress = false;
                ride.EndTime = DateTime.Now;
                _context.Entry(ride).State = EntityState.Modified;
                _context.SaveChanges();
            }

        }

        private async Task LaunchSaver()
        {
            IScheduler sched = await schedulerFactory.GetScheduler();
            await sched.Start();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<SaverJob>()
                .WithIdentity("saverJob", "group1")
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("saverTrigger", "group1")
              .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInSeconds(40)
                  .RepeatForever())
              .Build();

            await sched.ScheduleJob(job, trigger);
        }

        private async Task<string> MakeInferenceRequest(byte[] imgJpegEncoded)
        {
            //client.BaseAddress = new Uri("http://localost:8000");
            //var request = new HttpRequestMessage(HttpMethod.Post, "classifier/inference");
            //request.Content = new ByteArrayContent(imgJpegEncoded);
            //var r = await client.GetAsync("http://localhost:8000/classifier");
            var res = await _httpClient.PostAsync("http://localhost:8000/classifier/inference", new ByteArrayContent(imgJpegEncoded));

            return await res.Content.ReadAsStringAsync();
        }

        private class SaverJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                //return _context.SaveChangesAsync();
                AAPZ_BackendContext dbContext = (AAPZ_BackendContext)context.JobDetail.JobDataMap["_context"];

                return dbContext.SaveChangesAsync();
            }
        }
    }
}
