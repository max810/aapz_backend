using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using AAPZ_Backend.Providers;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;

namespace AAPZ_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private IConnectionManagerThreadSafe<string> _connManager;
        private HttpClient _httpClient = new HttpClient();
        public static AAPZ_BackendContext _context = null;
        private static IServiceProvider _provider;
        private static bool QuartzLaunched = false;
        public StreamController(IConnectionManagerThreadSafe<string> connManager, AAPZ_BackendContext context, IServiceProvider provider)
        {
            _connManager = connManager;
            _context = context;
            _provider = provider;
        }
        [HttpGet("start-stream")]
        public IActionResult StartStream(string ipaddr, string port)
        {
            IPEndPoint carAddr = new IPEndPoint(IPAddress.Parse(ipaddr), int.Parse(port));
            //Task.Run(receive udp dgrams && update _connManager.SetFrame());
            if (_connManager.IsAlive(carAddr.ToString()))
            {
                return Ok("Stream already exists");
            }
            TaskHelper.RunBgLong(async () =>
            {
                if (!QuartzLaunched)
                {
                    ISchedulerFactory schedFact = new StdSchedulerFactory();
                    IScheduler sched = await schedFact.GetScheduler();
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
                    QuartzLaunched = true;
                }
                int inferenceEveryFrames = 10;
                double increaseBy = inferenceEveryFrames / 30.0;
                double[] classes = new double[10];
                //AAPZ_BackendContext _context = new AAPZ_BackendContext(); //(AAPZ_BackendContext)_provider.GetService(typeof(AAPZ_BackendContext));
                UdpClient udpClient = new UdpClient(5005);
                bool connectionAlive = false;
                byte[] dgram = new byte[] { };
                string senderAddr = "-";
                Ride ride = new Ride();
                Driver driver = _context.Drivers.FirstOrDefault(x => x.UserId == 3); // TODO - find by address
                ride.Driver = driver;
                ride.DriverId = driver.UserId;
                _context.Add(ride);
                _context.SaveChanges();
                Console.WriteLine("Starting receiving stream");
                try
                {
                    int i = 0;
                    while (true)
                    {
                        // JUST connManager.GetFrame(...) and write it
                        // All other code to another method

                        //var clsTimer = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        Task frameAwaiter = Task.Delay(TimeSpan.FromSeconds(30));
                        Task frameReader = Task.Run(() =>
                        {
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
            });
            Response.StatusCode = 201;

            return Created("", null);
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
    }

    class SaverJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return StreamController._context.SaveChangesAsync();
        }
    }


}