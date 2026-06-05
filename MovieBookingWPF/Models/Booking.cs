using System;
using System.ComponentModel.DataAnnotations;

namespace MovieBookingWPF
{
    public class Booking
    {
        [Key] // Primary identity key for Entity Framework
        public int Id { get; set; }

        public string MovieTitle { get; set; } = string.Empty;

        public string Showtime { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string Seat { get; set; } = string.Empty;

        public string PaymentMode { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; } = DateTime.Now;
    }
}