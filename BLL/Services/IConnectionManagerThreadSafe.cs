using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IConnectionManagerThreadSafe<TIPAddr>
    {
        void SetAlive(TIPAddr addr);
        void SetZombie(TIPAddr addr);
        bool IsAlive(TIPAddr addr);
        byte[] GetFrame(TIPAddr addr);
        void SetFrame(TIPAddr addr, byte[] frame);
        int GetClassId(TIPAddr addr);
        void SetClassId(TIPAddr addr, int classId);
    }
}
