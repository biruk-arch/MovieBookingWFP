using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MovieBookingWPF.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<MovieItem> Movies { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}