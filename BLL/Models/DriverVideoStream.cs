using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class DriverVideoStream
    {
        public byte[] CurrentFrame { get; private set; }
        public Driver Driver { get; private set; }
        public int Port { get; private set; }
        public TimeSpan ListenerTimeout { get; set; }
        private UdpClient udpClient;
        public bool IsRunning { get; private set; } = false;

        public event FrameReceivedEventHandler FrameReceived;

        public DriverVideoStream(Driver driver, int port, TimeSpan listenerTimeout)
        {
            Driver = driver; // needed?
            Port = port;
            ListenerTimeout = listenerTimeout;
            udpClient = new UdpClient(Port);
        }

        public async Task Start(CancellationToken clsToken)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The listening video stream is already running");
            }
            else
            {
                IsRunning = true;
                udpClient = new UdpClient(Port); // needed?
            }
            //udpClient.
            IPEndPoint sender = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
            Console.WriteLine("Starting receiving stream");
            try
            {
                while (true)
                {
                    Task frameAwaiter = Task.Delay(ListenerTimeout);
                    Task frameReader = Task.Run(() =>
                    {
                        CurrentFrame = udpClient.Receive(ref sender);
                    });

                    await Task.WhenAny(frameReader, frameAwaiter);

                    if (!frameReader.IsCompleted && frameAwaiter.IsCompleted || clsToken.IsCancellationRequested)
                    {
                        // handle not receiving frames
                        IsRunning = false;
                        return;
                    }

                    FrameReceived?.Invoke(CurrentFrame);
                }
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}
