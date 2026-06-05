using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieBookingWPF
{
    public class MovieItem
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;

        // FIX: Added explicit SQL Server column formatting to clear the warning!
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Restored string[] so your UI doesn't break.
        // The [NotMapped] tag tells Entity Framework to ignore the array itself...
        [NotMapped]
        public string[] Showtimes { get; set; } = Array.Empty<string>();

        // ...And this property automatically converts that array into a single 
        // comma-separated string behind the scenes so SQL Server can store it!
        public string ShowtimesDatabase
        {
            get => string.Join(",", Showtimes ?? Array.Empty<string>());
            set => Showtimes = string.IsNullOrEmpty(value)
                ? Array.Empty<string>()
                : value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}