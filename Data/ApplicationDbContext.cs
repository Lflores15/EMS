using Microsoft.EntityFrameworkCore;
using EMS.Models;

namespace EMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
