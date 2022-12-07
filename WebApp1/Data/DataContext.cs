using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp1.Models.Identity;

namespace WebApp1.Data
{
    public class DataContext : IdentityDbContext<WebUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

   
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        public void DetachAllEntities()
        {
            if (this.ChangeTracker.Entries() != null)
                foreach (var entry in this.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }



        }
    }
}
