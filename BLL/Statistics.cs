﻿using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BLL.Models;

namespace BLL
{
    public class Statistics
    {
        private readonly AAPZ_BackendContext _context;

        public Statistics(AAPZ_BackendContext context)
        {
            _context = context;
        }

        public DriverStats GetDriverInfo(Driver driver)
        {
            return new DriverStats(driver);
        }

        public DriverStats GetDriverInfoFromPeriod(Driver driver, DateTime from, DateTime to)
        {
            return new DriverStats(driver, from, to);
        }
        
        public IList<DriverStats> GetRatingList (IEnumerable<Driver> drivers)
        {
            var ratedDrivers = new List<DriverStats>();
            foreach (var driver in drivers)
            {
                var stats = GetDriverInfo(driver);
                ratedDrivers.Add(stats);
            }

            return ratedDrivers.OrderByDescending(x => x.ClassesStatsScaled["0_normal_driving"]).ToList();
        }
    }
}
