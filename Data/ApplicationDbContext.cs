using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EMS.Models;
using Microsoft.AspNetCore.Identity;

namespace EMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // Use your custom User class
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {   
            // No need to explicitly add DbSet<User> since IdentityDbContext<User> already has a Users DbSet.
        }

        public DbSet<Event> Events { get; set; }
    }
}
