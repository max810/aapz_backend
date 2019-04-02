using AAPZ_Backend.Providers;
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

namespace AAPZ_Backend.SignalR
{
    //[Authorize(Roles = "admin")]
    public class StreamHub : Hub<IInferenceHub>
    {
        private IConnectionManagerThreadSafe<string> _connManager;
        public StreamHub(IConnectionManagerThreadSafe<string> connectionManager)
        {
            _connManager = connectionManager;
        }

        private HttpClient client = new HttpClient();

        public ChannelReader<byte[]> VideoStream(string addr)
        {
            var channel = Channel.CreateUnbounded<byte[]>();

            _ = WriteVideo(channel.Writer, addr);
            string userName = Context.User.Identity.Name;
            return channel.Reader;

        }
        private async Task WriteVideo(ChannelWriter<byte[]> writer, string addr)
        {
            //IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            //UdpClient client = new UdpClient(5005);
            //bool connectionAlive = false;
            byte[] dgram = null;
            //string senderAddr = "-";
            try
            {
                while (true)
                {
                    //_connManager.GetFrame(addr);   
                    // JUST connManager.GetFrame(...) and write it
                    // All other code to another method

                    //var clsTimer = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    Task frameAwaiter = Task.Delay(TimeSpan.FromSeconds(30));
                    Task frameReader = Task.Run(() =>
                    {
                        byte[] newDgram = null;
                        do
                        {
                            newDgram = _connManager.GetFrame(addr);
                        } while (newDgram == dgram);

                        dgram = newDgram;
                    });

                    await Task.WhenAny(frameReader, frameAwaiter);

                    if (!frameReader.IsCompleted && frameAwaiter.IsCompleted)
                    {
                        // handle not receiving frames
                        await Clients.Caller.InferenceMessage("-1");
                        //_connManager.SetZombie(senderAddr);
                        return;
                    }

                    await writer.WriteAsync(dgram);
                    await Clients.Caller.InferenceMessage(_connManager.GetClassId(addr).ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION: " + e.Message);
                throw e;
            }
            finally
            {
                //Clients.Clien().Close();
                writer.TryComplete();
            }
        }


        //public ChannelReader<byte[]> DelayCounter(int delay)
        //{
        //    var channel = Channel.CreateUnbounded<byte[]>();

        //    _ = WriteItems(channel.Writer, 20, delay);

        //    return channel.Reader;
        //}

        //private async Task WriteItems(ChannelWriter<byte[]> writer, int count, int delay)
        //{
        //    for (var i = 0; i < count; i++)
        //    {
        //        //For every 5 items streamed, add twice the delay
        //        if (i % 5 == 0)
        //            delay = delay * 2;

        //        await writer.WriteAsync(new byte[] { 1, 2, 3, (byte)i });
        //        await Task.Delay(delay);
        //    }

        //    writer.TryComplete();
        //}
    }
}
