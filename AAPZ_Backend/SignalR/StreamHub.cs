using BLL.Models;
using BLL.Models.Events;
using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BLL.SignalR
{
    //[Authorize(Roles = "admin")]
    public class StreamHub : Hub<IInferenceHub>
    {
        private StreamingLogic _streaming;
        private AAPZ_BackendContext _context;
        private Channel<byte[]> channel;
        public StreamHub(StreamingLogic streaming, AAPZ_BackendContext context)
        {
            _streaming = streaming;
            _context = context;
        }

        private HttpClient client = new HttpClient();

        public ChannelReader<byte[]> VideoStream(string driverId)
        {
            channel = Channel.CreateUnbounded<byte[]>();
            var driver = _context.Drivers.FirstOrDefault(x => x.Id == driverId);

            VideoStream stream;
            _streaming.TryGetStream(driver, out stream);
            string driverIdentifier = stream.DriverIdentifierHashB64;
            stream.FrameReceived += OnFrameReceived;
            stream.Closed += OnStreamClosed;

            //_ = WriteVideo(channel.Writer, driver);

            string userName = Context.User.Identity.Name;

            return channel.Reader;
        }

        //private async Task WriteVideo(ChannelWriter<byte[]> writer, Driver driver)
        //{
        //    //byte[] dgram = null;
        //    int classIdx = -1;


        //    //try
        //    //{
        //    //    while (true)
        //    //    {
        //    //        //_connManager.GetFrame(addr);   
        //    //        // JUST connManager.GetFrame(...) and write it
        //    //        // All other code to another method

        //    //        //var clsTimer = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        //    //        Task frameAwaiter = Task.Delay(TimeSpan.FromSeconds(30));
        //    //        Task frameReader = Task.Run(() =>
        //    //        {
        //    //            byte[] newDgram = null;
        //    //            do
        //    //            {
        //    //                newDgram = _connManager.GetFrame(driverId);
        //    //            } while (newDgram == dgram);

        //    //            dgram = newDgram;
        //    //        });

        //    //        await Task.WhenAny(frameReader, frameAwaiter);

        //    //        if (!frameReader.IsCompleted && frameAwaiter.IsCompleted)
        //    //        {
        //    //            // handle not receiving frames
        //    //            await Clients.Caller.InferenceMessage("-1");
        //    //            //_connManager.SetZombie(senderAddr);
        //    //            return;
        //    //        }

        //    //        await writer.WriteAsync(dgram);
        //    //        await Clients.Caller.InferenceMessage(_connManager.GetClassId(driverId).ToString());
        //    //    }
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Console.WriteLine("EXCEPTION: " + e.Message);
        //    //    throw e;
        //    //}
        //    //finally
        //    //{
        //    //    //Clients.Clien().Close();
        //    //    writer.TryComplete();
        //    //}
        //}

        private async void OnStreamClosed(object sender, StreamClosedEventArgs e)
        {
            channel.Writer.TryComplete();
            if(e.Status == DriverVideoStreamFinishStatus.Graceful)
            {
                await Clients.Caller.InferenceMessage("FINISHED SUCCESSFULY");
            }
            else
            {
                await Clients.Caller.InferenceMessage("ERROR HAPPENED ;)");
            }
        }

        private async void OnFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            VideoStream stream = sender as VideoStream;
            int currentClassIdx = _streaming.LastClassIdxs[stream.DriverIdentifierHashB64];

            await channel.Writer.WriteAsync(e.CurrentFrame);
            await Clients.Caller.InferenceMessage(currentClassIdx.ToString());
        }
    }
}
