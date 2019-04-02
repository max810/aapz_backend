using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class Driver
    {
        [Key]
        public int UserId { get; set; }
        public string License { get; set; }
        public int Age { get; set; }
        public int Experience { get; set; }
        public int CompanyId { get; set; }
        public int CarId { get; set; }

        public virtual IdentityUser User { get; set; }
        public virtual Company Company { get; set; }
        public virtual Car Car { get; set; }
    }
}
