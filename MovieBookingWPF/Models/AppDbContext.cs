using Microsoft.EntityFrameworkCore;

namespace MovieBookingWPF.Models
{
    public class AppDbContext : DbContext
    {
        // This links your C# model to a "Movies" table inside SQL Server
        public DbSet<MovieItem> Movies { get; set; }

        // This links your C# Booking model to a permanent "Bookings" table inside SQL Server
        public DbSet<Booking> Bookings { get; set; }

        // NEW FIX: This links your C# User model to a "Users" table for Admin & Customer login credentials
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Direct link to your local MovieBookingsDB database
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=MovieBookingsDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}