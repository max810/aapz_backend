using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AAPZ_Backend.SignalR
{
    public class StreamHub : Hub
    {
        public ChannelReader<byte[]> DelayCounter(int delay)
        {
            var channel = Channel.CreateUnbounded<byte[]>();

            _ = WriteItems(channel.Writer, 20, delay);

            return channel.Reader;
        }

        public ChannelReader<byte[]> VideoStream()
        {
            var channel = Channel.CreateBounded<byte[]>(1);

            _ = WriteVideo(channel.Writer);

            return channel.Reader;

        }

        private async Task WriteItems(ChannelWriter<byte[]> writer, int count, int delay)
        {
            for (var i = 0; i < count; i++)
            {
                //For every 5 items streamed, add twice the delay
                if (i % 5 == 0)
                    delay = delay * 2;

                await writer.WriteAsync(new byte[] { 1, 2, 3, (byte)i });
                await Task.Delay(delay);
            }

            writer.TryComplete();
        }

        private async Task WriteVideo(ChannelWriter<byte[]> writer)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            UdpClient client = new UdpClient(5005);
            try
            {
                while(true)
                {
                    // TODO : make async and wait 5 sec
                    byte[] data = client.Receive(ref ip);
                    await writer.WriteAsync(data);
                    //Image<Rgb24> img = Image.Load<Rgb24>(data);
                    //img.Save("img.jpg");
                }
            }
            finally
            {
                client.Close();
                writer.TryComplete();
            }
        }
    }
}
