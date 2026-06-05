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
    public class AdminDashboardWindow : Window
    {
        private StackPanel _moviesListPanel;
        private List<MovieItem> _movies;
        private List<Booking> _bookings;
        private Border _moviesBadgeElement;
        private Border _bookingsBadgeElement;
        private Border _card;
        private StackPanel _cardStack;
        private Button _addBtn;

        public AdminDashboardWindow()
        {
            Title = "Admin Panel - Movie Booking System";
            Width = 1200;
            Height = 720;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(246, 248, 251));

            // Initial Data Load directly from SQL tables
            LoadMoviesFromDatabase();
            LoadBookingsFromDatabase();

            MovieStore.BookingsChanged += () => {
                LoadBookingsFromDatabase();
                UpdateBadges();
            };

            var root = new Grid { Margin = new Thickness(16) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            var header = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
            var left = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            left.Children.Add(new Rectangle { Width = 36, Height = 24, Fill = new SolidColorBrush(Color.FromRgb(31, 53, 66)), RadiusX = 3, RadiusY = 3 });
            left.Children.Add(new TextBlock { Text = "  Admin Panel · Movie Booking System", FontSize = 20, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 32)), Margin = new Thickness(8, 0, 0, 0) });
            DockPanel.SetDock(left, Dock.Left);
            header.Children.Add(left);

            var right = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            right.Children.Add(new TextBlock { Text = "Admin", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
            var logout = CreatePillButton("Logout", Color.FromRgb(255, 255, 255), Brushes.Black, 96);
            logout.Click += (s, e) => { var login = new MainWindow(); login.Show(); this.Close(); };
            right.Children.Add(logout);
            DockPanel.SetDock(right, Dock.Right);
            header.Children.Add(right);

            Grid.SetRow(header, 0);
            root.Children.Add(header);

            // Main content container
            var content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });

            // Left: controls + list
            var leftStack = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
            var badges = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 12) };
            _moviesBadgeElement = CreateBadge($"Movies: {_movies.Count}");
            _bookingsBadgeElement = CreateBadge($"Total Bookings: {_bookings.Count}");
            badges.Children.Add(_moviesBadgeElement);
            badges.Children.Add(_bookingsBadgeElement);
            leftStack.Children.Add(badges);

            // Tabs
            var tabs = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            Border manageTab = (Border)CreateTab("Manage Movies", true);
            Border bookingsTab = (Border)CreateTab("All Bookings", false);
            manageTab.MouseLeftButtonUp += (s, e) => { ShowManageMovies(); SetActiveTab(manageTab, bookingsTab); };
            bookingsTab.MouseLeftButtonUp += (s, e) => { ShowAllBookings(); SetActiveTab(bookingsTab, manageTab); };
            tabs.Children.Add(manageTab);
            tabs.Children.Add(bookingsTab);
            leftStack.Children.Add(tabs);

            // Card area
            _card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(14) };
            _cardStack = new StackPanel();
            _addBtn = CreatePillButton("+ Add New Movie", Color.FromRgb(17, 41, 51), Brushes.White, 140);
            _addBtn.Margin = new Thickness(0, 0, 0, 12);

            _addBtn.Click += (s, e) => {
                var dlg = new AddMovieWindow { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    LoadMoviesFromDatabase();
                    PopulateMoviesList();
                    UpdateBadges();
                }
            };

            _moviesListPanel = new StackPanel { Orientation = Orientation.Vertical };
            PopulateMoviesList();
            ShowManageMovies();

            _card.Child = _cardStack;
            leftStack.Children.Add(_card);

            Grid.SetColumn(leftStack, 0);
            content.Children.Add(leftStack);

            // Right panel layout spacer
            var rightPanel = new StackPanel();
            rightPanel.Children.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(12), Height = 120 });
            Grid.SetColumn(rightPanel, 1);
            content.Children.Add(rightPanel);

            Grid.SetRow(content, 1);
            root.Children.Add(content);

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

        private void LoadBookingsFromDatabase()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _bookings = db.Bookings.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _bookings = new List<Booking>();
            }
        }

        private void PopulateMoviesList()
        {
            _moviesListPanel.Children.Clear();
            foreach (var m in _movies)
            {
                var rowBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(245, 247, 249)), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 12) };
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var leftComposite = new StackPanel { Orientation = Orientation.Horizontal };
                if (!string.IsNullOrEmpty(m.ImagePath) && File.Exists(m.ImagePath))
                {
                    try
                    {
                        var img = new Image { Width = 80, Height = 80, Margin = new Thickness(0, 0, 12, 0) };
                        img.Source = new BitmapImage(new Uri(m.ImagePath));
                        leftComposite.Children.Add(img);
                    }
                    catch { }
                }

                var left = new StackPanel();
                left.Children.Add(new TextBlock { Text = m.Title, FontWeight = FontWeights.Bold, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 32)) });
                left.Children.Add(new TextBlock { Text = m.Genre, Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)), Margin = new Thickness(0, 6, 0, 8) });
                leftComposite.Children.Add(left);

                var timesPanel = new StackPanel { Orientation = Orientation.Horizontal };
                if (m.Showtimes != null)
                {
                    foreach (var t in m.Showtimes)
                    {
                        timesPanel.Children.Add(CreateTimePill(t));
                    }
                }
                left.Children.Add(timesPanel);

                Grid.SetColumn(leftComposite, 0);
                g.Children.Add(leftComposite);

                var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                var edit = CreatePillButton("Edit", Color.FromRgb(255, 255, 255), Brushes.Black, 84);
                edit.Margin = new Thickness(0, 0, 8, 0);
                edit.Click += (s, e) => MessageBox.Show($"Edit {m.Title}", "Edit", MessageBoxButton.OK, MessageBoxImage.Information);
                actions.Children.Add(edit);

                var showtimes = CreatePillButton("Showtimes", Color.FromRgb(255, 255, 255), Brushes.Black, 100);
                showtimes.Margin = new Thickness(0, 0, 8, 0);
                showtimes.Click += (s, e) => MessageBox.Show($"Showtimes for {m.Title}: {string.Join(", ", m.Showtimes ?? new string[] { })}", "Showtimes", MessageBoxButton.OK, MessageBoxImage.Information);
                actions.Children.Add(showtimes);

                var delete = CreatePillButton("Delete", Color.FromRgb(220, 38, 38), Brushes.White, 88);
                delete.Click += (s, e) => {
                    if (MessageBox.Show($"Delete {m.Title}?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (var db = new AppDbContext())
                            {
                                var dbMovie = db.Movies.FirstOrDefault(x => x.Id == m.Id);
                                if (dbMovie != null)
                                {
                                    db.Movies.Remove(dbMovie);
                                    db.SaveChanges();
                                }
                            }
                            LoadMoviesFromDatabase();
                            PopulateMoviesList();
                            UpdateBadges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting movie: {ex.Message}");
                        }
                    }
                };
                actions.Children.Add(delete);

                Grid.SetColumn(actions, 1);
                g.Children.Add(actions);

                rowBorder.Child = g;
                _moviesListPanel.Children.Add(rowBorder);
            }
        }

        private void ShowManageMovies()
        {
            _cardStack.Children.Clear();
            _cardStack.Children.Add(_addBtn);
            PopulateMoviesList();
            _cardStack.Children.Add(_moviesListPanel);
        }

        private void ShowAllBookings()
        {
            _cardStack.Children.Clear();

            var titleText = new TextBlock { Text = "All Customer Reservations Database Records", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 12) };
            _cardStack.Children.Add(titleText);

            var listContainer = new StackPanel();
            LoadBookingsFromDatabase(); // Refresh rows live

            if (!_bookings.Any())
            {
                listContainer.Children.Add(new TextBlock { Text = "No global reservations recorded yet.", Foreground = Brushes.Gray, Margin = new Thickness(0, 8, 0, 0) });
            }
            else
            {
                foreach (var b in _bookings)
                {
                    var recordRow = new Border { Background = new SolidColorBrush(Color.FromRgb(245, 247, 249)), CornerRadius = new CornerRadius(8), Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8) };
                    var details = new StackPanel();
                    details.Children.Add(new TextBlock { Text = $"User: {b.CustomerName}", FontWeight = FontWeights.Bold });
                    details.Children.Add(new TextBlock { Text = $"Movie: {b.MovieTitle} | Room Seat: {b.Seat} | Time Slot: {b.Showtime}", FontSize = 12, Foreground = Brushes.DarkSlateGray });

                    recordRow.Child = details;
                    listContainer.Children.Add(recordRow);
                }
            }

            _cardStack.Children.Add(listContainer);
        }

        private void UpdateBadges()
        {
            if (_moviesBadgeElement != null && _movies != null)
                ((TextBlock)_moviesBadgeElement.Child).Text = $"Movies: {_movies.Count}";
            if (_bookingsBadgeElement != null && _bookings != null)
                ((TextBlock)_bookingsBadgeElement.Child).Text = $"Total Bookings: {_bookings.Count}";
        }

        private Border CreateBadge(string text)
        {
            var b = new Border { Background = new SolidColorBrush(Color.FromRgb(229, 231, 235)), CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(0, 0, 8, 0) };
            b.Child = new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeights.SemiBold };
            return b;
        }

        private UIElement CreateTab(string title, bool isActive)
        {
            var b = new Border { Cursor = System.Windows.Input.Cursors.Hand, Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 8, 0), BorderThickness = new Thickness(0, 0, 0, 2), BorderBrush = isActive ? new SolidColorBrush(Color.FromRgb(17, 41, 51)) : Brushes.Transparent };
            b.Child = new TextBlock { Text = title, FontWeight = isActive ? FontWeights.Bold : FontWeights.Normal, Foreground = isActive ? Brushes.Black : Brushes.Gray };
            return b;
        }

        private void SetActiveTab(Border active, Border inactive)
        {
            active.BorderBrush = new SolidColorBrush(Color.FromRgb(17, 41, 51));
            ((TextBlock)active.Child).FontWeight = FontWeights.Bold;
            ((TextBlock)active.Child).Foreground = Brushes.Black;

            inactive.BorderBrush = Brushes.Transparent;
            ((TextBlock)inactive.Child).FontWeight = FontWeights.Normal;
            ((TextBlock)inactive.Child).Foreground = Brushes.Gray;
        }

        private Button CreateTimePill(string text) => CreatePillButton(text, Color.FromRgb(240, 243, 247), Brushes.Black, 65);

        private Button CreatePillButton(string text, Color backgroundColor, Brush foreground, double width = double.NaN)
        {
            var btn = new Button { Content = text, Background = new SolidColorBrush(backgroundColor), Foreground = foreground, Padding = new Thickness(8, 4, 8, 4), BorderThickness = new Thickness(0), Height = 28, Margin = new Thickness(0, 0, 4, 0) };
            if (!double.IsNaN(width)) btn.Width = width;
            return btn;
        }
    }
}