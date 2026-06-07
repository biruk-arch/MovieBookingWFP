using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Imaging;
using MovieBookingWPF.Models;

namespace MovieBookingWPF
{
    public class CustomerDashboardWindow : Window
    {
        private string _userEmail;
        private StackPanel _myBookingsListPanel;
        private TextBlock _seatInfoText;
        private StackPanel _seatGridPanel;
        private MovieItem _selectedMovie;
        private string _selectedShowtime;
        private string _selectedSeat;
        private Button _selectedSeatButton;

        private List<MovieItem> _movies;

        public CustomerDashboardWindow(string userEmail = "")
        {
            _userEmail = userEmail ?? string.Empty;

            Title = "Customer Dashboard";
            Width = 1200;
            Height = 720;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(246, 248, 251));

            LoadMoviesFromDatabase();

            var root = new Grid { Margin = new Thickness(16) };

            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // HEADER
            var header = new DockPanel { Margin = new Thickness(0, 0, 0, 12), LastChildFill = false };
            var leftStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var rect = new Rectangle { Width = 36, Height = 24, Fill = new SolidColorBrush(Color.FromRgb(31, 53, 66)), RadiusX = 3, RadiusY = 3 };

            leftStack.Children.Add(rect);
            leftStack.Children.Add(new TextBlock
            {
                Text = "  CineDash - your seat, your story",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 32)),
                Margin = new Thickness(8, 0, 0, 0)
            });

            DockPanel.SetDock(leftStack, Dock.Left);
            header.Children.Add(leftStack);

            var logoutBtn = CreatePillButton("Logout", Color.FromRgb(220, 38, 38), Brushes.White, 120);
            logoutBtn.Click += LogoutBtn_Click;

            DockPanel.SetDock(logoutBtn, Dock.Right);
            header.Children.Add(logoutBtn);

            Grid.SetRow(header, 0);
            root.Children.Add(header);

            // MAIN CONTENT
            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // LEFT SIDE
            var leftColumn = new StackPanel();
            var leftScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var wrap = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(8), ItemWidth = 420, ItemHeight = 300 };

            foreach (var movie in _movies)
            {
                wrap.Children.Add(CreateMovieCard(movie));
            }

            MovieStore.MoviesChanged += () =>
            {
                LoadMoviesFromDatabase();
                wrap.Children.Clear();
                foreach (var movie in _movies)
                {
                    wrap.Children.Add(CreateMovieCard(movie));
                }
            };

            leftScroll.Content = wrap;
            leftColumn.Children.Add(leftScroll);

            Grid.SetColumn(leftColumn, 0);
            contentGrid.Children.Add(leftColumn);

            // RIGHT SIDEBAR
            var sidebarScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(18, 0, 0, 0) };
            var sidebar = new StackPanel();

            // SEAT SELECTION
            var seatSelectionBorder = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 14) };
            var seatSelectionStack = new StackPanel();

            seatSelectionStack.Children.Add(new TextBlock { Text = "Select your seats", FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 0, 0, 8) });

            _seatInfoText = new TextBlock { Text = "Select a movie first", Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)), FontSize = 11, Margin = new Thickness(0, 0, 0, 10) };
            seatSelectionStack.Children.Add(_seatInfoText);

            _seatGridPanel = new StackPanel();
            CreateSeatGrid(_seatGridPanel);
            seatSelectionStack.Children.Add(_seatGridPanel);

            var sidebarBookBtn = new Button { Content = "Book", Background = new SolidColorBrush(Color.FromRgb(17, 41, 51)), Foreground = Brushes.White, Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(0, 12, 0, 0), BorderThickness = new Thickness(0) };
            sidebarBookBtn.Click += SidebarBookBtn_Click;
            seatSelectionStack.Children.Add(sidebarBookBtn);

            seatSelectionBorder.Child = seatSelectionStack;
            sidebar.Children.Add(seatSelectionBorder);

            // MY BOOKINGS
            var bookingsBorder = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(12) };
            var bookingStack = new StackPanel();

            bookingStack.Children.Add(new TextBlock { Text = "My bookings", FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 0, 0, 8) });

            _myBookingsListPanel = new StackPanel();
            bookingStack.Children.Add(_myBookingsListPanel);
            bookingsBorder.Child = bookingStack;
            sidebar.Children.Add(bookingsBorder);

            MovieStore.BookingsChanged += () => { RefreshMyBookingsPanel(); };
            RefreshMyBookingsPanel();

            sidebarScroll.Content = sidebar;
            Grid.SetColumn(sidebarScroll, 1);
            contentGrid.Children.Add(sidebarScroll);

            Grid.SetRow(contentGrid, 1);
            root.Children.Add(contentGrid);

            Content = root;
        }

        private void LoadMoviesFromDatabase()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _movies = db.Movies.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading movies: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _movies = new List<MovieItem>();
            }
        }

        private void RefreshMyBookingsPanel()
        {
            if (_myBookingsListPanel == null) return;
            _myBookingsListPanel.Children.Clear();

            bool any = false;

            // FIX: Pulling fresh from database table record logs instead of runtime static properties
            try
            {
                using (var db = new AppDbContext())
                {
                    var savedBookings = db.Bookings
                        .Where(b => b.CustomerName.ToLower() == _userEmail.ToLower())
                        .ToList();

                    foreach (var booking in savedBookings)
                    {
                        any = true;
                        var row = new Border { Background = new SolidColorBrush(Color.FromRgb(245, 247, 249)), CornerRadius = new CornerRadius(8), Padding = new Thickness(8), Margin = new Thickness(0, 0, 0, 8) };
                        var stack = new StackPanel();

                        stack.Children.Add(new TextBlock { Text = booking.MovieTitle, FontWeight = FontWeights.Bold });
                        stack.Children.Add(new TextBlock { Text = $"Showtime: {booking.Showtime} | Seat: {booking.Seat}", FontSize = 12 });

                        row.Child = stack;
                        _myBookingsListPanel.Children.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                _myBookingsListPanel.Children.Add(new TextBlock { Text = $"Error loading history: {ex.Message}", Foreground = Brushes.Red });
                return;
            }

            if (!any)
            {
                _myBookingsListPanel.Children.Add(new TextBlock { Text = "No bookings yet.", Foreground = Brushes.Gray });
            }
        }

        private void SidebarBookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMovie == null) { MessageBox.Show("Please select a movie."); return; }
            if (string.IsNullOrEmpty(_selectedShowtime)) { MessageBox.Show("Please select a showtime."); return; }
            if (string.IsNullOrEmpty(_selectedSeat)) { MessageBox.Show("Please select a seat."); return; }

            var bookingWindow = new BookingWindow(_selectedMovie.Title, _selectedShowtime, _userEmail);

            if (bookingWindow.ShowDialog() == true && bookingWindow.CreatedBooking != null)
            {
                var booking = bookingWindow.CreatedBooking;
                booking.Seat = _selectedSeat;

                try
                {
                    using (var db = new AppDbContext())
                    {
                        var dbBooking = new Booking
                        {
                            CustomerName = string.IsNullOrEmpty(_userEmail) ? "Guest User" : _userEmail,
                            MovieTitle = _selectedMovie.Title,
                            Showtime = _selectedShowtime,
                            Seat = _selectedSeat
                        };

                        db.Bookings.Add(dbBooking);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Booking saved successfully to database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    RefreshMyBookingsPanel(); // Instantly displays database updates locally
                    ResetSeatSelection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save booking to database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private Border CreateMovieCard(MovieItem movie)
        {
            var border = new Border { Width = 420, Height = 280, CornerRadius = new CornerRadius(14), Background = Brushes.White, Margin = new Thickness(12) };
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var top = new Border { Height = 110, Background = new SolidColorBrush(Color.FromRgb(22, 40, 56)), CornerRadius = new CornerRadius(14, 14, 0, 0) };
            string displayTitle = "MOVIE";

            if (!string.IsNullOrEmpty(movie.Title))
            {
                string[] parts = movie.Title.Split(':');
                displayTitle = parts[0].Trim().ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(movie.ImagePath) && File.Exists(movie.ImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(movie.ImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    top.Background = new ImageBrush { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
                }
                catch
                {
                    top.Background = new SolidColorBrush(Color.FromRgb(22, 40, 56));
                }
            }

            var bigText = new TextBlock { Text = displayTitle, Foreground = Brushes.White, FontSize = 32, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            top.Child = bigText;

            Grid.SetRow(top, 0);
            grid.Children.Add(top);

            var bottomBorder = new Border { Background = Brushes.White, Padding = new Thickness(12) };
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock { Text = movie.Title, FontWeight = FontWeights.Bold, FontSize = 16 });
            stack.Children.Add(new TextBlock { Text = $"{movie.Genre} • {movie.Duration}", FontSize = 12, Margin = new Thickness(0, 4, 0, 6), Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)) });

            var timesPanel = new StackPanel { Orientation = Orientation.Horizontal };
            if (movie.Showtimes != null)
            {
                foreach (var time in movie.Showtimes)
                {
                    if (time != null)
                    {
                        string cleanedTime = time.Trim();
                        var timeBtn = CreateTimePill(cleanedTime);
                        timeBtn.Click += (s, e) => { UpdateSeatSelection(movie, cleanedTime); };
                        timesPanel.Children.Add(timeBtn);
                    }
                }
            }
            stack.Children.Add(timesPanel);

            var bookBtn = CreatePillButton("Book", Color.FromRgb(17, 41, 51), Brushes.White, 80);
            bookBtn.Margin = new Thickness(0, 10, 0, 0);
            bookBtn.Click += (s, e) =>
            {
                string firstTime = (movie.Showtimes != null && movie.Showtimes.Any()) ? movie.Showtimes.First().Trim() : "";
                UpdateSeatSelection(movie, firstTime);
            };
            stack.Children.Add(bookBtn);

            bottomBorder.Child = stack;
            Grid.SetRow(bottomBorder, 1);
            grid.Children.Add(bottomBorder);

            border.Child = grid;
            return border;
        }

        private Button CreateTimePill(string text) => CreatePillButton(text, Color.FromRgb(240, 243, 247), Brushes.Black, 65);

        private Button CreatePillButton(string text, Color backgroundColor, Brush foreground, double width = double.NaN)
        {
            var btn = new Button { Content = text, Background = new SolidColorBrush(backgroundColor), Foreground = foreground, Padding = new Thickness(8, 4, 8, 4), BorderThickness = new Thickness(0), Height = 28, Margin = new Thickness(0, 0, 4, 0) };
            if (!double.IsNaN(width)) btn.Width = width;
            return btn;
        }

        private void CreateSeatGrid(StackPanel container)
        {
            container.Children.Clear();
            string[] rows = { "A", "B", "C", "D" };

            foreach (var row in rows)
            {
                var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
                for (int i = 1; i <= 5; i++)
                {
                    string seat = $"{row}{i}";
                    var btn = new Button { Content = seat, Width = 35, Height = 35, Margin = new Thickness(3), Tag = seat };
                    btn.Click += (s, e) =>
                    {
                        if (_selectedSeatButton != null)
                        {
                            _selectedSeatButton.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                            _selectedSeatButton.Foreground = Brushes.Black;
                        }
                        btn.Background = new SolidColorBrush(Color.FromRgb(22, 40, 56));
                        btn.Foreground = Brushes.White;
                        _selectedSeat = seat;
                        _selectedSeatButton = btn;
                    };
                    rowPanel.Children.Add(btn);
                }
                container.Children.Add(rowPanel);
            }
        }

        private void ResetSeatSelection()
        {
            _selectedMovie = null; _selectedShowtime = null; _selectedSeat = null; _selectedSeatButton = null;
            _seatInfoText.Text = "Select a movie first";
            CreateSeatGrid(_seatGridPanel);
        }

        private void UpdateSeatSelection(MovieItem movie, string showtime)
        {
            _selectedMovie = movie; _selectedShowtime = showtime; _selectedSeat = null; _selectedSeatButton = null;
            _seatInfoText.Text = $"{movie.Title} — {showtime} — Select a seat";
            CreateSeatGrid(_seatGridPanel);
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var login = new MainWindow();
            login.Show();
            Close();
        }
    }
}