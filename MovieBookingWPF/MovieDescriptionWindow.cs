using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MovieBookingWPF
{
    public class MovieDescriptionWindow : Window
    {
        public MovieDescriptionWindow(string title, string genre)
        {
            Title = "Movie description";
            Width = 420;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Background = Brushes.Transparent;
            AllowsTransparency = true;

            var rootBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(18),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 20, Opacity = 0.15, ShadowDepth = 6 }
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(15,23,32)) });
            stack.Children.Add(new TextBlock { Text = $"Genre: {genre}", Margin = new Thickness(0,8,0,0), Foreground = new SolidColorBrush(Color.FromRgb(107,114,128)) });
            stack.Children.Add(new TextBlock { Text = "\nDescription:\nThis is a sample description for the selected movie. Replace this with actual movie synopsis.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0), Foreground = new SolidColorBrush(Color.FromRgb(55,65,81)) });

            var closeBtn = new Button { Content = "Close", Width = 96, Height = 34, Margin = new Thickness(0,14,0,0), HorizontalAlignment = HorizontalAlignment.Right, Background = new SolidColorBrush(Color.FromRgb(15,23,32)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };
            // apply rounded template
            try
            {
                var xaml = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'><Border CornerRadius='14' Background='{TemplateBinding Background}' Padding='6'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center' /></Border></ControlTemplate>";
                closeBtn.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
            }
            catch { }

            closeBtn.Click += (s, e) => this.Close();
            stack.Children.Add(closeBtn);

            rootBorder.Child = stack;
            Content = rootBorder;
        }
    }
}
