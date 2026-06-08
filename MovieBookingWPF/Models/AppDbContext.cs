using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // Required for password hashing

namespace MovieBookingWPF.Models
{
    public class AppDbContext : DbContext
    {
        // This links your C# model to a "Movies" table inside SQL Server
        public DbSet<MovieItem> Movies { get; set; }

        // This links your C# Booking model to a permanent "Bookings" table inside SQL Server
        public DbSet<Booking> Bookings { get; set; }

        // This links your C# User model to a "Users" table for Admin & Customer login credentials
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Direct link to your local MovieBookingsDB database
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=MovieBookingsDB;Trusted_Connection=True;TrustServerCertificate=True;");
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