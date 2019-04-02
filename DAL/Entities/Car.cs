using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public Status Status{ get; set; }

        public virtual Driver Driver { get; set; }
    }
}
