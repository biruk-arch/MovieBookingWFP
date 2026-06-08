using Microsoft.EntityFrameworkCore;

namespace MovieBookingWPF.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<MovieItem> Movies { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        // NEW FIX: This links your C# User model to a "Users" table for Admin & Customer login credentials
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed an Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@movie.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"), // Hashes "admin123" securely
                    Role = "Admin"
                },
                // Seed a Regular Customer User
                new User
                {
                    Id = 2,
                    Email = "customer@movie.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("user123"), // Hashes "user123" securely
                    Role = "Customer"
                }
            );
        }
    }
}