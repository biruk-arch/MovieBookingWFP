using System;
using System.ComponentModel.DataAnnotations;

namespace MovieBookingWPF
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // In a production app, you'd hash this!

        [Required]
        public string Role { get; set; } = "Customer"; // Default role: "Customer" or "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}