using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Ride
    {
        public int RideId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; } = null;
        public bool InProgress { get; set; } = true;
        public int DriverId { get; set; }
        public int NormalDrivingSeconds { get; set; }
        public int MobileRightSeconds { get; set; }
        public int MobileRightHeadSeconds { get; set; }
        public int MobileLeftSeconds { get; set; }
        public int MobileLeftHeadSeconds { get; set; }
        public int RadioSeconds { get; set; }
        public int DrinkSeconds { get; set; }
        public int SearchSeconds { get; set; }
        public int MakeupSeconds { get; set; }
        public int TalkingSeconds { get; set; }

        public virtual Driver Driver { get; set; }

        public void IncrementDrivingClass(int classId)
        {
            switch (classId)
            {
                case 0:
                    ++NormalDrivingSeconds;
                    break;
                case 1:
                    ++MobileRightSeconds;
                    break;
                case 2:
                    ++MobileRightHeadSeconds;
                    break;
                case 3:
                    ++MobileLeftSeconds;
                    break;
                case 4:
                    ++MobileLeftHeadSeconds;
                    break;
                case 5:
                    ++RadioSeconds;
                    break;
                case 6:
                    ++DrinkSeconds;
                    break;
                case 7:
                    ++SearchSeconds;
                    break;
                case 8:
                    ++MakeupSeconds;
                    break;
                case 9:
                    ++TalkingSeconds;
                    break;
                default:
                    throw new ArgumentException(string.Format("No such class: {0}", classId));
            }
        }
    }
}
