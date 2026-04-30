using System.Collections.Generic;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieBookingWPF
{
    public class AllBookingsWindow : Window
    {
        public AllBookingsWindow(List<Booking> bookings)
        {
            Title = "All Bookings";
            Width = 560;
            Height = 420;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.Transparent;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;

            var root = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = "Bookings", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,8) });

            if (bookings == null || bookings.Count == 0)
            {
                stack.Children.Add(new TextBlock { Text = "No bookings yet.", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)) });
            }
            else
            {
                var list = new StackPanel();
                foreach (var b in bookings)
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
                    var del = new Button { Content = "Delete", Background = new SolidColorBrush(Color.FromRgb(220,38,38)), Foreground = Brushes.White, Padding = new Thickness(10,4,10,4), BorderThickness = new Thickness(0) };
                    del.Click += (s, e) => { bookings.Remove(b); this.Close(); new AllBookingsWindow(bookings).ShowDialog(); };
                    actions.Children.Add(del);
                    Grid.SetColumn(actions, 1);
                    g.Children.Add(actions);

                    row.Child = g;
                    list.Children.Add(row);
                }
                stack.Children.Add(list);
            }

            var close = new Button { Content = "Close", HorizontalAlignment = HorizontalAlignment.Right, Padding = new Thickness(12,6,12,6), Margin = new Thickness(0,12,0,0) };
            close.Click += (s,e) => this.Close();
            stack.Children.Add(close);

            root.Child = stack;
            Content = root;
        }
    }
}
