using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models.Events
{
    public enum DriverVideoStreamFinishStatus
    {
        Graceful = 0x0,
        Timeout = 0x1,
        ConnectionBroken = 0x2, 
    }
}
