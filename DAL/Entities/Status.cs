using System;

namespace DAL.Entities
{
    [Flags]
    public enum Status
    {
        OK = 0x0,
        NotWorking = 0x1,
        Unknown = 0x2,
    }
}