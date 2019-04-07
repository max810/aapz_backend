using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models.Events
{
    public class StreamStartedEventArgs: EventArgs
    {
        public int Port { get; private set; }

        public StreamStartedEventArgs(int port)
        {
            Port = port;
        }
    }
}
