using BLL.Models;
using BLL.Models.Events;
using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BLL
{
    public class StreamingLogic
    {
        public ConcurrentDictionary<string, int> LastClassIdxs => lastClassIdxs;
        private static ConcurrentDictionary<string, int> lastClassIdxs = new ConcurrentDictionary<string, int>();
        private static bool SaverLaunched = false;
        private readonly AAPZ_BackendContext _context;
        private readonly HttpClient _httpClient = new HttpClient();

        private static readonly ConcurrentDictionary<string, CancellationTokenSource> VideoStreamCls = new ConcurrentDictionary<string, CancellationTokenSource>(); // use this
        private static readonly Stack<int> freePorts = new Stack<int>(Enumerable.Range(6000, 65535 - 6000).Reverse());
        private static readonly ConcurrentDictionary<string, VideoStream> VideoStreams = new ConcurrentDictionary<string, VideoStream>();
        private static readonly ConcurrentDictionary<string, Ride> CurrentRides = new ConcurrentDictionary<string, Ride>();
        private static readonly ISchedulerFactory schedulerFactory = new StdSchedulerFactory();

        public StreamingLogic(AAPZ_BackendContext context)
        {
            _context = context;
        }

        public bool TryGetStream(Driver driver, out VideoStream videoStream)
        {
            return VideoStreams.TryGetValue(driver.IdentifierHashB64, out videoStream);
        }

        public IEnumerable<VideoStream> GetActiveStreams()
        {
            return VideoStreams.Values.Where(x => x.IsRunning);
        }

        //public bool TryGetStream(string driverIdentifierHashB64, out VideoStream videoStream)
        //{
        //    return VideoStreams.TryGetValue(driverIdentifierHashB64, out videoStream);
        //}

        public bool StreamExists(Driver driver)
        {
            return VideoStreams.TryGetValue(driver.IdentifierHashB64, out VideoStream stream) && stream.IsRunning;
        }

        //public bool StreamExists(string driverIdentifierHashB64)
        //{
        //    return VideoStreams.TryGetValue(driverIdentifierHashB64, out VideoStream stream) && stream.IsRunning;
        //}

        public void StopStream(Driver driver)
        {
            if (VideoStreamCls.ContainsKey(driver.IdentifierHashB64))
            {
                VideoStreamCls[driver.IdentifierHashB64].Cancel();
            }
        }

        //public void StopStream(string driverIdentifierHashB64)
        //{
        //    VideoStreamCls[driverIdentifierHashB64].Cancel();
        //}

        //public int StartNewListeningStream(Driver driver, TimeSpan listeningTimeout)
        //{
        //    return StartNewListeningStream(driver.IdentifierHashB64, listeningTimeout);
        //}

        public int StartNewListeningStream(Driver driver, TimeSpan listeningTimeout)
        {
            if (!SaverLaunched)
            {
                LaunchSaver().Wait();
                SaverLaunched = true;
            }
            int listeningPort = GrabNextFreePort();

            var stream = StartNewListeningStream(driver, listeningPort, listeningTimeout);
            
            // listeningPort is now occupied
            return listeningPort;
        }

        private int GrabNextFreePort()
        {
            return freePorts.Pop();
        }

        private void FreePort(int port)
        {
            freePorts.Push(port);
        }

        //// maybe void?
        //private async Task StartNewListeningStream(Driver driver, int port, TimeSpan listeningTimeout)
        //{
        //    await StartNewListeningStream(driver.IdentifierHashB64, port, listeningTimeout);
        //}

        private VideoStream StartNewListeningStream(Driver driver, int port, TimeSpan listeningTimeout, bool withInference = true)
        {
            string driverId = driver.IdentifierHashB64;
            var stream = new VideoStream(driverId, port, listeningTimeout);

            //string driverId = driver.IdentifierHashB64;
            VideoStreams.AddOrUpdate(driverId, stream, (_, __) => stream);

            var cls = new CancellationTokenSource();
            VideoStreamCls.AddOrUpdate(driverId, cls, (_, __) => cls);

            Ride ride = new Ride()
            {
                Driver = driver,
                DriverId = driver.Id,
                //StartTime = DateTime.Now,
                InProgress = true,
            };

            _context.Rides.Add(ride);
            //_context.SaveChanges();

            stream.Started += (s, e) => ride.StartTime = DateTime.Now;

            stream.Closed += (s, e) =>
            {
                FreePort((s as VideoStream).Port);
                if (withInference)
                {
                    ride.EndTime = DateTime.Now;
                    ride.InProgress = false;
                    _context.Entry(ride).State = EntityState.Modified;
                    _context.SaveChanges();

                    // TODO - check why Cancel on token doesn't work and it waits for timeout anyway
                }
            };

            if (withInference)
            {
                stream.FrameReceived += OnFrameReceived;
            }

            TaskHelper.RunBgLong(() =>
                stream.Start(cls.Token)
                .Wait());

            CurrentRides[driverId] = ride;

            return stream;
        }

        private async void OnFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            VideoStream stream = sender as VideoStream;
            if (stream.FramesReceived % 10 == 0)
            {
                int classIdx = await MakeInferenceRequest(e.CurrentFrame);
                Ride ride = CurrentRides[stream.DriverIdentifierHashB64];

                LastClassIdxs[stream.DriverIdentifierHashB64] = classIdx;
                ride.IncreaseClassCount(classIdx, 10.0 / 30.0);
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
            
            job.JobDataMap["_context"] = _context;

            // Trigger the job to run now, and then every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("saverTrigger", "group1")
              .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInSeconds(10)
                  .RepeatForever())
              .Build();

            await sched.ScheduleJob(job, trigger);
        }

        private async Task<int> MakeInferenceRequest(byte[] imgJpegEncoded)
        {
            //TODO - inject Configuration and get classifier url from there
            //client.BaseAddress = new Uri("http://localost:8000");
            //var request = new HttpRequestMessage(HttpMethod.Post, "classifier/inference");
            //request.Content = new ByteArrayContent(imgJpegEncoded);
            //var r = await client.GetAsync("http://localhost:8000/classifier");
            var res = await _httpClient.PostAsync("http://localhost:8000/classifier/inference", new ByteArrayContent(imgJpegEncoded));

            return int.Parse(await res.Content.ReadAsStringAsync());
        }

        private class SaverJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                //return _context.SaveChangesAsync();
                AAPZ_BackendContext dbContext = (AAPZ_BackendContext)context.JobDetail.JobDataMap["_context"];
                Console.WriteLine("\n\n\nSAVING\n\n");
                return dbContext.SaveChangesAsync();
            }
        }
    }

}
