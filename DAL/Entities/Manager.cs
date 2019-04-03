using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class Manager
    {
        [Key]
        public int UserId { get; set; }
        public string Certificate { get; set; }
        public string FullName { get; set; }
        public int CompanyId { get; set; }

        public virtual User User { get; set; }
        public virtual Company Company { get; set; }
    }
}
