using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
    }
}
