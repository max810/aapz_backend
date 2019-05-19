using BLL.Models.Events;
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
    public class VideoStream
    {
        //public byte[] CurrentFrame { get; private set; }
        public int FramesReceived { get; private set; } = 0;
        public string DriverIdentifierHashB64 { get; private set; }
        public int Port { get; private set; }
        public TimeSpan ListenerTimeout { get; set; }
        private UdpClient udpClient;
        public bool IsRunning { get; private set; } = false;
        private DriverVideoStreamFinishStatus closedStatus;

        public event EventHandler<StreamStartedEventArgs> Started;
        public event EventHandler<FrameReceivedEventArgs> FrameReceived;
        public event EventHandler<StreamClosedEventArgs> Closed;

        public VideoStream(Driver driver, int port, TimeSpan listenerTimeout)
            :this(driver.IdentifierHashB64, port, listenerTimeout)
        {
        }

        public VideoStream(string driverIdentifierHashB64, int port, TimeSpan listenerTimeout)
        {
            DriverIdentifierHashB64 = driverIdentifierHashB64; 
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
                //udpClient = new UdpClient(Port); // needed? will we possibly restart streams after they are closed?
            }

            IPEndPoint sender = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
            Console.WriteLine("Starting receiving stream");

            Started?.Invoke(this, new StreamStartedEventArgs(Port));

            try
            {
                while (true)
                {
                    byte[] currentFrame = null;
                    Task frameAwaiter = Task.Delay(ListenerTimeout);
                    Task frameReader = Task.Run(() =>
                    {
                        currentFrame = udpClient.Receive(ref sender);
                    });
                    if (clsToken.IsCancellationRequested)
                    {
                        closedStatus = DriverVideoStreamFinishStatus.Graceful;
                        IsRunning = false;
                        return;
                    }

                    await Task.WhenAny(frameReader, frameAwaiter);

                    bool timeout = !frameReader.IsCompleted && frameAwaiter.IsCompleted;
                    if (timeout)
                    {
                        closedStatus = DriverVideoStreamFinishStatus.Timeout;
                        IsRunning = false;
                        return;
                    }

                    ++FramesReceived;
                    FrameReceived?.Invoke(this, new FrameReceivedEventArgs(currentFrame));
                }
            }
            catch(Exception e)
            {
                closedStatus = DriverVideoStreamFinishStatus.ConnectionBroken;
                throw e;
            }
            finally
            {
                udpClient.Close();
                Closed?.Invoke(this, new StreamClosedEventArgs(closedStatus));
            }
        }
    }
}
