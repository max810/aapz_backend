using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DAL.Entities
{
    public class Driver
    {
        [Key]
        //[ForeignKey("User")]
        public int UserId { get; set; }

        public string IdentifierHashB64 { get; set; }
        public int Age { get; set; }
        public int Experience { get; set; }
        public int CompanyId { get; set; }

        public virtual User User { get; set; }
        public virtual Company Company { get; set; }
    }
}
