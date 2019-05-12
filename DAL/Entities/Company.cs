using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class Company
    {
        //public int Id { get; set; }
        [Key]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string LogoB64 { get; set; }

        public int NumberOfEmployees
        {
            get => (Drivers?.Count ?? 0) + (Managers?.Count ?? 0);
            private set { }
        }

        public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
        public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();
    }
}
