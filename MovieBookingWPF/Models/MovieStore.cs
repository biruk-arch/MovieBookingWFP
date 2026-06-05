using MovieBookingWPF.Models; // <-- ADD THIS LINE
using System;
using System.Collections.Generic;

namespace MovieBookingWPF
{
    public static class MovieStore
    {
        public static List<MovieItem> Movies { get; } = new List<MovieItem>();
        public static List<Booking> Bookings { get; } = new List<Booking>();

        public static event Action MoviesChanged;
        public static event Action BookingsChanged;

        static MovieStore()
        {
            // initialize some default movies
            Movies.Add(new MovieItem { Title = "Dune: Prophecy", Genre = "Sci-Fi / Adventure", Showtimes = new[] { "14:30", "18:45", "21:00" }, Price = 9.99m, Duration = "2h 35m" });
            Movies.Add(new MovieItem { Title = "Gladiator 2", Genre = "Action / Epic", Showtimes = new[] { "13:00", "16:15", "20:30" }, Price = 8.50m, Duration = "2h 10m" });
            Movies.Add(new MovieItem { Title = "The Wild Robot", Genre = "Animation / Family", Showtimes = new[] { "11:00", "15:20", "18:00" }, Price = 7.25m, Duration = "1h 40m" });
            Movies.Add(new MovieItem { Title = "Nosferatu", Genre = "Horror / Gothic", Showtimes = new[] { "19:45", "22:15" }, Price = 6.75m, Duration = "1h 55m" });
        }

        public static void AddMovie(MovieItem m)
        {
            Movies.Add(m);
            MoviesChanged?.Invoke();
        }

        public static void RemoveMovie(MovieItem m)
        {
            Movies.Remove(m);
            MoviesChanged?.Invoke();
        }

        public static void AddBooking(Booking b)
        {
            Bookings.Add(b);
            BookingsChanged?.Invoke();
        }

        public static void RemoveBooking(Booking b)
        {
            Bookings.Remove(b);
            BookingsChanged?.Invoke();
        }
    }
}
