using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models.Events
{
    public class StreamClosedEventArgs : EventArgs
    {
        public DriverVideoStreamFinishStatus Status { get; set; }

        public StreamClosedEventArgs(DriverVideoStreamFinishStatus status)
        {
            Status = status;
        }
    }
}
