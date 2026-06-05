using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieBookingWPF
{
    public partial class SeatSelectionWindow : Window
    {
        private MovieItem _movie;
        private string _showtime;
        private string _userEmail;
        private string _selectedSeat;
        private Grid _seatGrid;

        public SeatSelectionWindow(MovieItem movie, string showtime, string userEmail)
        {
            _movie = movie;
            _showtime = showtime;
            _userEmail = userEmail;
            _selectedSeat = null;

            Title = "Select Your Seat";
            Width = 500;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.White;
            WindowStyle = WindowStyle.SingleBorderWindow;

            var root = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(20) };

            // Title
            root.Children.Add(new TextBlock
            {
                Text = _movie.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Showtime info
            root.Children.Add(new TextBlock
            {
                Text = $"Showtime: {_showtime}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                Margin = new Thickness(0, 0, 0, 16)
            });

            // Seat selection label
            root.Children.Add(new TextBlock
            {
                Text = "Select your seat:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            });

            // Seat grid
            _seatGrid = CreateSeatGrid();
            root.Children.Add(_seatGrid);

            // Selected seat display
            var selectedSeatText = new TextBlock
            {
                Text = "No seat selected",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                Margin = new Thickness(0, 12, 0, 0)
            };
            root.Children.Add(selectedSeatText);

            // Action buttons
            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            try
            {
                var cancelXaml = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='6' Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border></ControlTemplate>";
                cancelBtn.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(cancelXaml);
            }
            catch { }

            cancelBtn.Click += (s, e) => this.Close();

            var confirmBtn = new Button
            {
                Content = "Book",
                Background = new SolidColorBrush(Color.FromRgb(17, 41, 51)),
                Foreground = Brushes.White,
                Padding = new Thickness(12, 6, 12, 6),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            try
            {
                var bookXaml = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='6' Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border></ControlTemplate>";
                confirmBtn.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(bookXaml);
            }
            catch { }

            confirmBtn.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(_selectedSeat))
                {
                    MessageBox.Show("Please select a seat.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var bookingWindow = new BookingWindow(_movie.Title, _showtime, _userEmail);
                if (bookingWindow.ShowDialog() == true && bookingWindow.CreatedBooking != null)
                {
                    var booking = bookingWindow.CreatedBooking;
                    booking.Seat = _selectedSeat;
                    MovieStore.AddBooking(booking);
                    this.Close();
                }
            };

            buttonRow.Children.Add(cancelBtn);
            buttonRow.Children.Add(confirmBtn);
            root.Children.Add(buttonRow);

            Content = root;
        }

        private Grid CreateSeatGrid()
        {
            var grid = new Grid();
            var rows = new[] { "A", "B", "C", "D" };
            var cols = 5;

            for (int i = 0; i < rows.Length; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int j = 0; j < cols; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var seatLabel = $"{rows[i]}{j + 1}";
                    var btn = new Button
                    {
                        Content = seatLabel,
                        Width = 50,
                        Height = 50,
                        Margin = new Thickness(6),
                        Background = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                        Foreground = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219))
                    };

                    btn.Click += (s, e) =>
                    {
                        // Reset previous selection
                        foreach (var child in grid.Children.OfType<Button>())
                        {
                            child.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                        }

                        // Set new selection
                        btn.Background = new SolidColorBrush(Color.FromRgb(22, 40, 56));
                        btn.Foreground = Brushes.White;
                        _selectedSeat = seatLabel;
                    };

                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                    grid.Children.Add(btn);
                }
            }

            return grid;
        }
    }
}
