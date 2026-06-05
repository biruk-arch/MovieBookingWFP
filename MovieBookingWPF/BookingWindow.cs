using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieBookingWPF
{
    public class BookingWindow : Window
    {
        private TextBox _nameBox;
        public Booking CreatedBooking { get; private set; }

        public BookingWindow(string movieTitle, string showtime, string defaultName = "")
        {
            Title = "Confirm booking";
            Width = 420;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.Transparent;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;

            var root = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(16) };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = movieTitle, FontWeight = FontWeights.Bold, FontSize = 16 });
            stack.Children.Add(new TextBlock { Text = $"Showtime: {showtime}", Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)), Margin = new Thickness(0,6,0,12) });

            stack.Children.Add(new TextBlock { Text = "Your name", FontWeight = FontWeights.SemiBold });
            _nameBox = new TextBox { Margin = new Thickness(0,6,0,12), Text = defaultName };
            stack.Children.Add(_nameBox);

            var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "Cancel", Margin = new Thickness(0,0,8,0), Padding = new Thickness(12,6,12,6), BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand };
            cancel.Click += (s, e) => { this.DialogResult = false; this.Close(); };
            var save = new Button { Content = "Confirm", Background = new SolidColorBrush(Color.FromRgb(17,41,51)), Foreground = Brushes.White, Padding = new Thickness(12,6,12,6), BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand };

            try
            {
                var buttonTemplate = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='6' Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border></ControlTemplate>";
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(buttonTemplate);
                cancel.Template = template;
                save.Template = template;
            }
            catch { }

            save.Click += (s, e) => {
                var name = _nameBox.Text?.Trim();
                if (string.IsNullOrEmpty(name)) { MessageBox.Show("Please enter your name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                CreatedBooking = new Booking { MovieTitle = movieTitle, Showtime = showtime, CustomerName = name };
                this.DialogResult = true; this.Close();
            };
            row.Children.Add(cancel);
            row.Children.Add(save);
            stack.Children.Add(row);

            root.Child = stack;
            Content = root;
        }
    }
}
