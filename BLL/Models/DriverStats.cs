using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL.Models
{
    public struct DriverStats
    {
        public Driver Driver { get; private set; }
        public int RidesTotal { get; private set; }
        private IEnumerable<Ride> correctRides;
        public DrivingStyle DrivingStyle { get; private set; }
        public IDictionary<string, double> ClassesStats { get; private set; }
        public IDictionary<string, double> ClassesStatsScaled { get; private set; }

        public DriverStats(Driver driver, DateTime from, DateTime to)
        {
            Driver = driver;

            correctRides = driver.Rides
                .Where(x => x.InProgress == false)
                .Where(x => x.StartTime >= from && x.EndTime <= to);

            RidesTotal = correctRides.Count();
            ClassesStats = CreateClassesStats(correctRides, from, to);
            ClassesStatsScaled = ScaleClassesStats(ClassesStats);
            DrivingStyle = EvaluateDrivingStyle(ClassesStats);
        }

        public DriverStats(Driver driver)
            : this(driver, DateTime.MinValue, DateTime.Now)
        {

        }

        private static IDictionary<string, double> CreateClassesStats(IEnumerable<Ride> rides, DateTime from, DateTime to)
        {
            //var correctRides = rides
            //    .Where(x => x.InProgress == false)
            //    .Where(x => x.StartTime >= from && x.EndTime <= to);

            var classesStats = new Dictionary<string, double>();

            // TODO - extract <...>Seconds into separate struct with [Owned]
            // TODO - replace with Rides.Sum(x => x.SeparateStruct)
            // Would also be possible to just iterate over all properties of SeparateStruct

            foreach(Ride ride in rides)
            {
                classesStats["0_normal_driving"] = classesStats.GetValueOrDefault("0_normal_driving", 0.0) + ride.NormalDrivingSeconds;
                classesStats["1_mobile_right"] = classesStats.GetValueOrDefault("1_mobile_right", 0.0) + ride.MobileRightSeconds;
                classesStats["2_mobile_right_head"] = classesStats.GetValueOrDefault("2_mobile_right_head", 0.0) + ride.MobileRightHeadSeconds;
                classesStats["3_mobile_left"] = classesStats.GetValueOrDefault("3_mobile_left", 0.0) + ride.MobileLeftSeconds;
                classesStats["4_mobile_left_head"] = classesStats.GetValueOrDefault("4_mobile_left_head", 0.0) + ride.MobileLeftHeadSeconds;
                classesStats["5_radio"] = classesStats.GetValueOrDefault("5_radio", 0.0) + ride.RadioSeconds;
                classesStats["6_drink"] = classesStats.GetValueOrDefault("6_drink", 0.0) + ride.DrinkSeconds;
                classesStats["7_search"] = classesStats.GetValueOrDefault("7_search", 0.0) + ride.SearchSeconds;
                classesStats["8_makeup"] = classesStats.GetValueOrDefault("8_makeup", 0.0) + ride.MakeupSeconds;
                classesStats["9_talking"] = classesStats.GetValueOrDefault("9_talking", 0.0) + ride.TalkingSeconds;
            }

            return classesStats;
        }

        private static IDictionary<string, double> ScaleClassesStats(IDictionary<string, double> classesStats)
        {
            var scaledClassesStats = new Dictionary<string, double>();
            double totalSeconds = classesStats.Values.Sum();

            foreach(var kv in classesStats)
            {
                scaledClassesStats[kv.Key] = kv.Value / totalSeconds;
            }

            return scaledClassesStats;
        }

        private static DrivingStyle EvaluateDrivingStyle(IDictionary<string, double> scaledClassesStats)
        {
            double normalDrivingFactor = scaledClassesStats["0_normal_driving"];
            if (normalDrivingFactor >= 0.8)
            {
                return DrivingStyle.Safe;
            }

            if(normalDrivingFactor >= 0.7)
            {
                return DrivingStyle.SlightlyCareless;
            }

            if(normalDrivingFactor >= 0.6)
            {
                return DrivingStyle.Careless;
            }

            return DrivingStyle.Reckless;
        }
    }
}
