using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Ride
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; } = null;
        public bool InProgress { get; set; } = true;
        public string DriverId { get; set; }
        public double NormalDrivingSeconds { get; set; }
        public double MobileRightSeconds { get; set; }
        public double MobileRightHeadSeconds { get; set; }
        public double MobileLeftSeconds { get; set; }
        public double MobileLeftHeadSeconds { get; set; }
        public double RadioSeconds { get; set; }
        public double DrinkSeconds { get; set; }
        public double SearchSeconds { get; set; }
        public double MakeupSeconds { get; set; }
        public double TalkingSeconds { get; set; }

        public virtual Driver Driver { get; set; }

        public void IncreaseClassCount(int classId, double value)
        {
            switch (classId)
            {
                case 0:
                    NormalDrivingSeconds += value;
                    break;
                case 1:
                    MobileRightSeconds += value;
                    break;
                case 2:
                    MobileRightHeadSeconds += value;
                    break;
                case 3:
                    MobileLeftSeconds += value;
                    break;
                case 4:
                    MobileLeftHeadSeconds += value;
                    break;
                case 5:
                    RadioSeconds += value;
                    break;
                case 6:
                    DrinkSeconds += value;
                    break;
                case 7:
                    SearchSeconds += value;
                    break;
                case 8:
                    MakeupSeconds += value;
                    break;
                case 9:
                    TalkingSeconds += value;
                    break;
                default:
                    throw new ArgumentException(string.Format("No such class: {0}", classId));
            }
        }
    }
}
