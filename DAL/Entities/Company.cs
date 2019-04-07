using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
        public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();
    }
}
