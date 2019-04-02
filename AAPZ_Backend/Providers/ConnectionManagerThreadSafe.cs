using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AAPZ_Backend.Providers
{
    public class ConnectionManagerThreadSafe<TIPAddr> : IConnectionManagerThreadSafe<TIPAddr>
    {
        public static readonly ConcurrentDictionary<TIPAddr, bool> connections = new ConcurrentDictionary<TIPAddr, bool>();
        public static readonly ConcurrentDictionary<TIPAddr, byte[]> currentVideoFrames = new ConcurrentDictionary<TIPAddr, byte[]>();
        public static readonly ConcurrentDictionary<TIPAddr, int> currentClassIds = new ConcurrentDictionary<TIPAddr, int>();

        public void SetAlive(TIPAddr addr)
        {
            connections.AddOrUpdate(addr, true, (k, v) => true);
        }

        public void SetZombie(TIPAddr addr)
        {
            connections.AddOrUpdate(addr, false, (k, v) => false);
        }

        public byte[] GetFrame(TIPAddr addr)
        {
            return currentVideoFrames.GetValueOrDefault(addr, null);
        }

        public void SetFrame(TIPAddr addr, byte[] frame)
        {
            currentVideoFrames.AddOrUpdate(addr, frame, (_, _1) => frame);
        }

        public bool IsAlive(TIPAddr addr)
        {
            return connections.GetValueOrDefault(addr, false);
        }

        public int GetClassId(TIPAddr addr)
        {
            return currentClassIds.GetValueOrDefault(addr, -1);
        }

        public void SetClassId(TIPAddr addr, int classId)
        {
            currentClassIds.AddOrUpdate(addr, classId, (_, _1) => classId);
        }
    }
}
