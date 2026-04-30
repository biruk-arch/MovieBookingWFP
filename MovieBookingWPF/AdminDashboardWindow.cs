using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Imaging;

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

            // use shared store
            _movies = MovieStore.Movies;
            _bookings = MovieStore.Bookings;
            MovieStore.MoviesChanged += () => { PopulateMoviesList(); UpdateBadges(); };
            MovieStore.BookingsChanged += () => { UpdateBadges(); if (_card != null) { /* refresh bookings view if visible */ } };

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
            // user label and logout
            right.Children.Add(new TextBlock { Text = "Admin", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,8,0) });
            var logout = CreatePillButton("Logout", Color.FromRgb(255,255,255), Brushes.Black, 96);
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
            // badges
            var badges = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 12) };
            _moviesBadgeElement = CreateBadge($"Movies: {_movies.Count}");
            _bookingsBadgeElement = CreateBadge($"Total Bookings: {_bookings.Count}");
            badges.Children.Add(_moviesBadgeElement);
            badges.Children.Add(_bookingsBadgeElement);
            leftStack.Children.Add(badges);

            // Tabs (simple text tabs)
            var tabs = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            // create tab elements and keep references so we can toggle active state
            Border manageTab = (Border)CreateTab("Manage Movies", true);
            Border bookingsTab = (Border)CreateTab("All Bookings", false);
            // handlers
            manageTab.MouseLeftButtonUp += (s, e) => { ShowManageMovies(); SetActiveTab(manageTab, bookingsTab); };
            bookingsTab.MouseLeftButtonUp += (s, e) => { ShowAllBookings(); SetActiveTab(bookingsTab, manageTab); };
            tabs.Children.Add(manageTab);
            tabs.Children.Add(bookingsTab);
            leftStack.Children.Add(tabs);

            // card area (used for manage movies or bookings)
            _card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(14) };
            _cardStack = new StackPanel();
            _addBtn = CreatePillButton("+ Add New Movie", Color.FromRgb(17,41,51), Brushes.White, 140);
            _addBtn.Margin = new Thickness(0, 0, 0, 12);
            _addBtn.Click += (s, e) => {
                var dlg = new AddMovieWindow();
                dlg.Owner = this;
                var result = dlg.ShowDialog();
                if (result == true && dlg.CreatedMovie != null)
                {
                    _movies.Add(dlg.CreatedMovie);
                    PopulateMoviesList();
                    // update badge
                    UpdateBadges();
                }
            };

            _moviesListPanel = new StackPanel { Orientation = Orientation.Vertical };
            PopulateMoviesList();

            // show manage movies view by default
            ShowManageMovies();

            _card.Child = _cardStack;
            leftStack.Children.Add(_card);

            Grid.SetColumn(leftStack, 0);
            content.Children.Add(leftStack);

            // Right: placeholder (bookings/summary)
            var rightPanel = new StackPanel();
            rightPanel.Children.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(12), Height = 120 });
            Grid.SetColumn(rightPanel, 1);
            content.Children.Add(rightPanel);

            Grid.SetRow(content, 1);
            root.Children.Add(content);

            Content = root;
        }

        private void PopulateMoviesList()
        {
            _moviesListPanel.Children.Clear();
            foreach (var m in _movies)
            {
                var rowBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(245,247,249)), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(0,0,0,12) };
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // show optional thumbnail on the left and details on the right
                var leftComposite = new StackPanel { Orientation = Orientation.Horizontal };
                if (!string.IsNullOrEmpty(m.ImagePath) && File.Exists(m.ImagePath))
                {
                    try
                    {
                        var img = new Image { Width = 80, Height = 80, Margin = new Thickness(0,0,12,0) };
                        img.Source = new BitmapImage(new System.Uri(m.ImagePath));
                        leftComposite.Children.Add(img);
                    }
                    catch { }
                }

                var left = new StackPanel();
                left.Children.Add(new TextBlock { Text = m.Title, FontWeight = FontWeights.Bold, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(15,23,32)) });
                left.Children.Add(new TextBlock { Text = m.Genre, Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), Margin = new Thickness(0,6,0,8) });
                leftComposite.Children.Add(left);

                var timesPanel = new StackPanel { Orientation = Orientation.Horizontal };
                foreach (var t in m.Showtimes)
                {
                    timesPanel.Children.Add(CreateTimePill(t));
                }
                left.Children.Add(timesPanel);

                Grid.SetColumn(leftComposite, 0);
                g.Children.Add(leftComposite);

                var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                var edit = CreatePillButton("Edit", Color.FromRgb(255,255,255), Brushes.Black, 84);
                edit.Margin = new Thickness(0,0,8,0);
                edit.Click += (s, e) => MessageBox.Show($"Edit {m.Title}", "Edit", MessageBoxButton.OK, MessageBoxImage.Information);
                actions.Children.Add(edit);

                var showtimes = CreatePillButton("Showtimes", Color.FromRgb(255,255,255), Brushes.Black, 100);
                showtimes.Margin = new Thickness(0,0,8,0);
                showtimes.Click += (s, e) => MessageBox.Show($"Showtimes for {m.Title}: {string.Join(", ", m.Showtimes)}", "Showtimes", MessageBoxButton.OK, MessageBoxImage.Information);
                actions.Children.Add(showtimes);

                var delete = CreatePillButton("Delete", Color.FromRgb(220,38,38), Brushes.White, 88);
                delete.Click += (s, e) => {
                    if (MessageBox.Show($"Delete {m.Title}?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        _movies.Remove(m);
                        PopulateMoviesList();
                        UpdateBadges();
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
            // ensure movies list is repopulated
            PopulateMoviesList();
            _cardStack.Children.Add(_moviesListPanel);
        }

        private void ShowAllBookings()
        {
            _cardStack.Children.Clear();
            var title = new TextBlock { Text = "All Bookings", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,8) };
            _cardStack.Children.Add(title);

            if (_bookings == null || _bookings.Count == 0)
            {
                _cardStack.Children.Add(new TextBlock { Text = "No bookings yet.", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)) });
                return;
            }

            var list = new StackPanel();
            foreach (var b in _bookings.ToList())
            {
                var row = new Border { Background = new SolidColorBrush(Color.FromRgb(245,247,249)), CornerRadius = new CornerRadius(8), Padding = new Thickness(10), Margin = new Thickness(0,0,0,8) };
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var left = new StackPanel();
                left.Children.Add(new TextBlock { Text = b.MovieTitle, FontWeight = FontWeights.SemiBold });
                left.Children.Add(new TextBlock { Text = $"Showtime: {b.Showtime} - Customer: {b.CustomerName}", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), FontSize = 12 });
                g.Children.Add(left);

                var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                var del = CreatePillButton("Delete", Color.FromRgb(220,38,38), Brushes.White, 88);
                del.Click += (s, e) => { if (MessageBox.Show($"Delete booking for {b.MovieTitle}?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) { _bookings.Remove(b); ShowAllBookings(); UpdateBadges(); } };
                actions.Children.Add(del);
                Grid.SetColumn(actions, 1);
                g.Children.Add(actions);

                row.Child = g;
                list.Children.Add(row);
            }

            _cardStack.Children.Add(list);
        }

        private Border CreateBadge(string text)
        {
            var b = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(20), Padding = new Thickness(12,8,12,8), Margin = new Thickness(0,0,12,0) };
            b.Child = new TextBlock { Text = text, Foreground = new SolidColorBrush(Color.FromRgb(15,23,32)) };
            return b;
        }

        private FrameworkElement CreateTab(string text, bool active)
        {
            var tb = new Border { Background = active ? new SolidColorBrush(Color.FromRgb(17,41,51)) : Brushes.Transparent, CornerRadius = new CornerRadius(18), Padding = new Thickness(12,8,12,8), Margin = new Thickness(0,0,8,0) };
            tb.Child = new TextBlock { Text = text, Foreground = active ? Brushes.White : new SolidColorBrush(Color.FromRgb(15,23,32)) };
            tb.Cursor = System.Windows.Input.Cursors.Hand;
            return tb;
        }

        private void UpdateBadges()
        {
            if (_moviesBadgeElement != null)
            {
                (_moviesBadgeElement.Child as TextBlock).Text = $"Movies: {_movies.Count}";
            }
            if (_bookingsBadgeElement != null)
            {
                (_bookingsBadgeElement.Child as TextBlock).Text = $"Total Bookings: {_bookings.Count}";
            }
        }

        private void SetActiveTab(Border active, Border inactive)
        {
            if (active != null)
            {
                active.Background = new SolidColorBrush(Color.FromRgb(17, 41, 51));
                var tb = active.Child as TextBlock;
                if (tb != null) tb.Foreground = Brushes.White;
            }
            if (inactive != null)
            {
                inactive.Background = Brushes.Transparent;
                var tb2 = inactive.Child as TextBlock;
                if (tb2 != null) tb2.Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 32));
            }
        }

        private void OpenAllBookings()
        {
            var win = new AllBookingsWindow(_bookings);
            win.Owner = this;
            win.ShowDialog();
            // refresh badge after potential deletes
            UpdateBadges();
        }

        private Button CreateTimePill(string text)
        {
            return CreatePillButton(text, Color.FromRgb(240,243,247), Brushes.Black, double.NaN);
        }

        private Button CreatePillButton(string text, Color backgroundColor, Brush foreground, double width = double.NaN)
        {
            var btn = new Button
            {
                Content = text,
                Background = new SolidColorBrush(backgroundColor),
                Foreground = foreground,
                Padding = new Thickness(10, 4, 10, 4),
                BorderThickness = new Thickness(0),
                Height = 32,
                Margin = new Thickness(0,0,8,0)
            };

            if (!double.IsNaN(width)) btn.Width = width;

            try
            {
                var xaml = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='18' Background='{TemplateBinding Background}' Padding='4'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border></ControlTemplate>";
                btn.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
            }
            catch { }

            return btn;
        }
    }
}
