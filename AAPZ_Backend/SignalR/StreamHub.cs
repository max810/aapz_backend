using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AAPZ_Backend.SignalR
{
    public class StreamHub : Hub<IInferenceHub>
    {

        private HttpClient client = new HttpClient();

        public ChannelReader<byte[]> VideoStream()
        {
            var channel = Channel.CreateUnbounded<byte[]>();

            _ = WriteVideo(channel.Writer);

            return channel.Reader;

        }
        private async Task WriteVideo(ChannelWriter<byte[]> writer)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            UdpClient client = new UdpClient(5005);
            try
            {
                int i = 0;
                while (true)
                {
                    var dgram = await client.ReceiveAsync();
                    await writer.WriteAsync(dgram.Buffer);

                    if (i % 10 == 0)
                    {
                        i = 0;
                        int classLabel = int.Parse(await MakeInferenceRequest(dgram.Buffer));
                        await Clients.Caller.InferenceMessage(classLabel);
                        //await Task.Run(async () =>
                        //{

                        //});
                    }
                    ++i;

                    //Task.Run(async () => await writer.WriteAsync(dgram.Buffer));
                    //.ContinueWith(res => writer.WriteAsync(res.Result.Buffer)))
                    //.ConfigureAwait(false);
                    ////UdpReceiveResult res = await client.ReceiveAsync();
                    //Task.Run(() => writer.WriteAsync(res.Buffer));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION: " + e.Message);
                throw e;
            }
            finally
            {
                client.Close();
                writer.TryComplete();
            }
        }

        private async Task<string> MakeInferenceRequest(byte[] imgJpegEncoded)
        {
            //client.BaseAddress = new Uri("http://localost:8000");
            //var request = new HttpRequestMessage(HttpMethod.Post, "classifier/inference");
            //request.Content = new ByteArrayContent(imgJpegEncoded);
            //var r = await client.GetAsync("http://localhost:8000/classifier");
            var res = await client.PostAsync("http://localhost:8000/classifier/inference", new ByteArrayContent(imgJpegEncoded));

            return await res.Content.ReadAsStringAsync();
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
