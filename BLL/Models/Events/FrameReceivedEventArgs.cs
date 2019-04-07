using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models.Events
{
    public class FrameReceivedEventArgs: EventArgs
    {
        public byte[] CurrentFrame { get; }

        public FrameReceivedEventArgs(byte[] currentFrame)
        {
            CurrentFrame = currentFrame;
        }
    }
}
