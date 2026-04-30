using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MovieBookingWPF
{
    public class CustomerDashboardWindow : Window
    {
        private string _userEmail;
        private StackPanel _myBookingsListPanel;

        public CustomerDashboardWindow(string userEmail = "")
        {
            _userEmail = userEmail ?? string.Empty;
            Title = "Customer Dashboard";
            Width = 1200;
            Height = 720;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(246, 248, 251));

            var root = new Grid { Margin = new Thickness(16) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            var header = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
            header.LastChildFill = false;
            var leftStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var rect = new Rectangle { Width = 36, Height = 24, Fill = new SolidColorBrush(Color.FromRgb(31,53,66)), RadiusX = 3, RadiusY = 3 };
            leftStack.Children.Add(rect);
            leftStack.Children.Add(new TextBlock { Text = "  CineDash - your seat, your story", FontSize = 20, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(15,23,32)), Margin = new Thickness(8,0,0,0) });
            DockPanel.SetDock(leftStack, Dock.Left);
            header.Children.Add(leftStack);

            var logoutBtn = CreatePillButton("Alex Rivera - logout", Color.FromRgb(220, 38, 38), Brushes.White, 140);
            logoutBtn.Margin = new Thickness(0, 0, 6, 0);
            logoutBtn.HorizontalAlignment = HorizontalAlignment.Right;
            logoutBtn.VerticalAlignment = VerticalAlignment.Center;
            logoutBtn.Click += LogoutBtn_Click;
            DockPanel.SetDock(logoutBtn, Dock.Right);
            header.Children.Add(logoutBtn);

            Grid.SetRow(header, 0);
            root.Children.Add(header);

            // Content grid
            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Left area - sample movie cards
            var leftColumn = new StackPanel { Orientation = Orientation.Vertical };

            // Movie grid
            var leftScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var wrap = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(8), ItemWidth = 420, ItemHeight = 300, HorizontalAlignment = HorizontalAlignment.Left };

            // populate from store
            foreach (var m in MovieStore.Movies)
            {
                wrap.Children.Add(CreateMovieCard(m));
            }

            MovieStore.MoviesChanged += () =>
            {
                // refresh list
                wrap.Children.Clear();
                foreach (var mm in MovieStore.Movies) wrap.Children.Add(CreateMovieCard(mm));
            };

            leftScroll.Content = wrap;
            leftColumn.Children.Add(leftScroll);
            Grid.SetColumn(leftColumn, 0);
            contentGrid.Children.Add(leftColumn);

            // Right sidebar - seat selection area (top) and My bookings below
            var sidebar = new StackPanel { Margin = new Thickness(18,0,0,0) };

            var myBookingsBorder = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(12), Margin = new Thickness(0,14,0,0), Height = 260 };
            var myBookingsStack = new StackPanel();
            myBookingsStack.Children.Add(new TextBlock { Text = "My bookings", FontWeight = FontWeights.Bold, FontSize = 16, Margin = new Thickness(0,0,0,8) });
            _myBookingsListPanel = new StackPanel();
            myBookingsStack.Children.Add(_myBookingsListPanel);
            myBookingsBorder.Child = myBookingsStack;
            sidebar.Children.Add(myBookingsBorder);

            MovieStore.BookingsChanged += () => RefreshMyBookingsPanel();
            RefreshMyBookingsPanel();

            Grid.SetColumn(sidebar, 1);
            contentGrid.Children.Add(sidebar);

            Grid.SetRow(contentGrid, 1);
            root.Children.Add(contentGrid);

            Content = root;
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var login = new MainWindow();
            login.Show();
            this.Close();
        }

        private Border CreateMovieCard(MovieItem movie)
        {
            var border = new Border { Width = 420, Height = 280, CornerRadius = new CornerRadius(14), Background = Brushes.White, Margin = new Thickness(12), Padding = new Thickness(0) };
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var top = new Border { Background = new SolidColorBrush(Color.FromRgb(22,40,56)), CornerRadius = new CornerRadius(14,14,0,0), Height = 110 };
            var big = new TextBlock { Text = movie.Title.Split(':')[0].ToUpper(), Foreground = Brushes.White, FontSize = 40, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            top.Child = big;
            Grid.SetRow(top, 0);
            grid.Children.Add(top);

            var borderStack = new Border { Background = Brushes.White, Padding = new Thickness(12) };
            var innerStack = new StackPanel();
            innerStack.Children.Add(new TextBlock { Text = movie.Title, FontWeight = FontWeights.Bold, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(15,23,32)) });
            innerStack.Children.Add(new TextBlock { Text = movie.Genre, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), Margin = new Thickness(0,4,0,6) });

            // short description
            innerStack.Children.Add(new TextBlock { Text = "An epic tale of adventure and survival in a world beyond imagination.", TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Color.FromRgb(55,65,81)), FontSize = 12, Margin = new Thickness(0,0,0,8), MaxWidth = 280 });

            // build a two-column row: left = showtime pills, right = actions (info + book)
            var actionRow = new Grid();
            actionRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actionRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var times = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            foreach (var t in movie.Showtimes)
            {
                var timeBtn = CreateTimePill(t);
                // when time clicked, open seat selection popup for this movie/showtime
                timeBtn.Click += (s, e) =>
                {
                    var win = new SeatSelectionWindow(movie, t, _userEmail);
                    win.Owner = this;
                    win.ShowDialog();
                };
                times.Children.Add(timeBtn);
            }

            Grid.SetColumn(times, 0);
            actionRow.Children.Add(times);

            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var descBtn = CreateIconButton("ℹ", Color.FromRgb(21,40,47), Brushes.White);
            descBtn.Margin = new Thickness(12, 0, 8, 0);
            descBtn.Click += (s, e) =>
            {
                var win = new MovieDescriptionWindow(movie.Title, movie.Genre);
                win.Owner = this;
                win.ShowDialog();
            };
            actionsPanel.Children.Add(descBtn);

            var bookBtn = CreatePillButton("Book", Color.FromRgb(17,41,51), Brushes.White, 84);
            bookBtn.Click += (s, e) =>
            {
                var win = new SeatSelectionWindow(movie, null, _userEmail);
                win.Owner = this;
                win.ShowDialog();
            };
            actionsPanel.Children.Add(bookBtn);

            Grid.SetColumn(actionsPanel, 1);
            actionRow.Children.Add(actionsPanel);

            innerStack.Children.Add(actionRow);

            borderStack.Child = innerStack;
            Grid.SetRow(borderStack, 1);
            grid.Children.Add(borderStack);

            border.Child = grid;
            return border;
        }

        private Button CreateTimePill(string text)
        {
            return CreatePillButton(text, Color.FromRgb(240,243,247), Brushes.Black, 80);
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
                Height = 28
            };

            if (!double.IsNaN(width)) btn.Width = width;

            // simple rounded template using XAML parsing
            try
            {
                var xaml = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='14' Background='{TemplateBinding Background}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border></ControlTemplate>";
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
                btn.Template = template;
            }
            catch
            {
                // ignore template errors and fall back to default button
            }

            return btn;
        }

        // Seat selection and confirmation are handled in SeatSelectionWindow (popup)

        private void RefreshMyBookingsPanel()
        {
            if (_myBookingsListPanel == null) return;
            _myBookingsListPanel.Children.Clear();
            var any = false;
            foreach (var b in MovieStore.Bookings.ToArray())
            {
                if (!string.IsNullOrEmpty(_userEmail) && b.CustomerName.Equals(_userEmail, System.StringComparison.OrdinalIgnoreCase))
                {
                    any = true;
                    var row = new Border { Background = new SolidColorBrush(Color.FromRgb(245,247,249)), CornerRadius = new CornerRadius(8), Padding = new Thickness(8), Margin = new Thickness(0,0,0,8) };
                    var g = new Grid();
                    g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var left = new StackPanel();
                    left.Children.Add(new TextBlock { Text = b.MovieTitle, FontWeight = FontWeights.SemiBold });
                    left.Children.Add(new TextBlock { Text = $"Showtime: {b.Showtime} — Seat: {b.Seat}", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), FontSize = 12 });
                    left.Children.Add(new TextBlock { Text = $"Booked by: {b.CustomerName}", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), FontSize = 11 });
                    if (!string.IsNullOrEmpty(b.PaymentMode)) left.Children.Add(new TextBlock { Text = $"Payment: {b.PaymentMode}", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), FontSize = 11 });
                    g.Children.Add(left);

                    var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                    var cancel = CreatePillButton("Cancel", Color.FromRgb(220,38,38), Brushes.White, 80);
                    cancel.Click += (s, e) =>
                    {
                        if (MessageBox.Show($"Cancel booking for {b.MovieTitle} (seat {b.Seat})?", "Cancel booking", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MovieStore.RemoveBooking(b);
                            RefreshMyBookingsPanel();
                        }
                    };
                    actions.Children.Add(cancel);
                    Grid.SetColumn(actions, 1);
                    g.Children.Add(actions);

                    row.Child = g;
                    _myBookingsListPanel.Children.Add(row);
                }
            }

            if (!any)
            {
                _myBookingsListPanel.Children.Add(new TextBlock { Text = "No tickets yet. Book a movie!", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)) });
            }
        }

        private Button CreateIconButton(string glyph, Color backgroundColor, Brush foreground)
        {
            var btn = new Button
            {
                Content = new TextBlock { Text = glyph, FontSize = 14, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                Background = new SolidColorBrush(backgroundColor),
                Foreground = foreground,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                Width = 36,
                Height = 36
            };

            try
            {
                var xaml = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='18' Background='{TemplateBinding Background}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border></ControlTemplate>";
                btn.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
            }
            catch { }

            return btn;
        }
    }
}
