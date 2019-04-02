using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace AAPZ_Backend
{
    public class ReturningMemoryStream : MemoryStream
    {
        Func<byte[]> newDataLoader;
        public ReturningMemoryStream(Func<byte[]> dataRetriever): base()
        {
            newDataLoader = dataRetriever;
            Capacity = 16384;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int res = base.Read(buffer, offset, count);
            if(Position == Length)
            {
                LoadNewData();
            }
            Seek(0, SeekOrigin.Begin);

            return res;
        }

        private void LoadNewData()
        {
            byte[] newData = newDataLoader?.Invoke();
            if(newData != null)
            {
                int dataLen = newData.Length;
                Write(newData, 0, dataLen);
                SetLength(dataLen);
                Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
