using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DAL.Entities
{
    public class Manager
    {
        [ForeignKey("User")]
        public string Id { get; set; }
        public string Certificate { get; set; }
        public string FullName { get; set; }
        public int CompanyId { get; set; }

        public virtual User User { get; set; }
        public virtual Company Company { get; set; }
    }
}
