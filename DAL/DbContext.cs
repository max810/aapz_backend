using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL
{
    public class AAPZ_BackendContext: IdentityDbContext<User>
    {
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Ride> Rides { get; set; }

        public AAPZ_BackendContext(DbContextOptions<AAPZ_BackendContext> dbContextOptions)
            : base(dbContextOptions)
        {
            //Database.EnsureCreated();
            //Database.Migrate();
        }
    }
}
