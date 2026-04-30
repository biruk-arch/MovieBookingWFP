using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace MovieBookingWPF
{
    public class AddMovieWindow : Window
    {
        private TextBox _titleBox;
        private TextBox _genreBox;
        private TextBox _durationBox;
        private TextBox _showtimesBox;
        private Image _previewImage;
        private string _imagePath;
        private Button _verifyBtn;
        private bool _isVerified = false;
        public MovieItem CreatedMovie { get; private set; }

        public AddMovieWindow()
        {
            Title = "Add New Movie";
            Width = 420;
            Height = 300;
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

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Children.Add(new TextBlock { Text = "Title", FontWeight = FontWeights.SemiBold });
            _titleBox = new TextBox { Margin = new Thickness(0,6,0,10) };
            stack.Children.Add(_titleBox);

            stack.Children.Add(new TextBlock { Text = "Genre", FontWeight = FontWeights.SemiBold });
            _genreBox = new TextBox { Margin = new Thickness(0,6,0,10) };
            stack.Children.Add(_genreBox);

            stack.Children.Add(new TextBlock { Text = "Duration (e.g. 2h 10m)", FontWeight = FontWeights.SemiBold });
            _durationBox = new TextBox { Margin = new Thickness(0,6,0,10) };
            stack.Children.Add(_durationBox);

            stack.Children.Add(new TextBlock { Text = "Showtimes (comma separated)", FontWeight = FontWeights.SemiBold });
            _showtimesBox = new TextBox { Margin = new Thickness(0,6,0,12) };
            stack.Children.Add(_showtimesBox);

            // Image picker
            stack.Children.Add(new TextBlock { Text = "Poster image (optional)", FontWeight = FontWeights.SemiBold });
            var imgRow = new StackPanel { Orientation = Orientation.Horizontal };
            _previewImage = new Image { Width = 64, Height = 96, Margin = new Thickness(0,6,12,12) };
            var chooseBtn = new Button { Content = "Choose image...", Padding = new Thickness(10,6,10,6) };
            chooseBtn.Click += (s, e) =>
            {
                var dlg = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*" };
                if (dlg.ShowDialog() == true)
                {
                    _imagePath = dlg.FileName;
                    try
                    {
                        _previewImage.Source = new BitmapImage(new System.Uri(_imagePath));
                    }
                    catch { }
                }
            };
            imgRow.Children.Add(_previewImage);
            imgRow.Children.Add(chooseBtn);
            stack.Children.Add(imgRow);

            var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            // verify button - must click before saving
            Button saveBtn = null;
            _verifyBtn = new Button { Content = "Verify", Margin = new Thickness(0,0,8,0), Padding = new Thickness(12,6,12,6), Background = new SolidColorBrush(Color.FromRgb(240,243,247)), Foreground = Brushes.Black };
            _verifyBtn.Click += (s, e) => {
                // basic validation
                var title = _titleBox.Text?.Trim();
                var duration = _durationBox.Text?.Trim();
                if (string.IsNullOrEmpty(title)) { MessageBox.Show("Please enter a title before verifying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                if (string.IsNullOrEmpty(duration)) { MessageBox.Show("Please enter duration before verifying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                // mark verified
                _isVerified = true;
                _verifyBtn.IsEnabled = false;
                _verifyBtn.Background = new SolidColorBrush(Color.FromRgb(34,197,94));
                _verifyBtn.Foreground = Brushes.White;
                // enable Save after verification
                saveBtn.IsEnabled = true;
            };

            var back = new Button { Content = "Back", Margin = new Thickness(0,0,8,0), Padding = new Thickness(12,6,12,6) };
            back.Click += (s,e) => { this.DialogResult = false; this.Close(); };
            saveBtn = new Button { Content = "Save", Background = new SolidColorBrush(Color.FromRgb(17,41,51)), Foreground = Brushes.White, Padding = new Thickness(12,6,12,6), IsEnabled = false };
            saveBtn.Click += Save_Click;
            var doneBtn = new Button { Content = "Done", Margin = new Thickness(8,0,0,0), Padding = new Thickness(12,6,12,6), Background = new SolidColorBrush(Color.FromRgb(17,41,51)), Foreground = Brushes.White };
            doneBtn.Click += (s, e) => {
                // perform save without requiring verification
                var title = _titleBox.Text?.Trim();
                var genre = _genreBox.Text?.Trim();
                var showtimesText = _showtimesBox.Text?.Trim();
                var duration = _durationBox.Text?.Trim();
                if (string.IsNullOrEmpty(title)) { MessageBox.Show("Please enter a title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var showtimes = new string[] { };
                if (!string.IsNullOrEmpty(showtimesText))
                {
                    showtimes = showtimesText.Split(',');
                    for (int i = 0; i < showtimes.Length; i++) showtimes[i] = showtimes[i].Trim();
                }
                CreatedMovie = new MovieItem { Title = title, Genre = genre ?? string.Empty, Showtimes = showtimes, ImagePath = _imagePath, Duration = duration ?? string.Empty };
                this.DialogResult = true;
                this.Close();
            };
            row.Children.Add(_verifyBtn);
            row.Children.Add(back);
            row.Children.Add(saveBtn);
            row.Children.Add(doneBtn);
            stack.Children.Add(row);

            root.Child = stack;
            Content = root;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var title = _titleBox.Text?.Trim();
            var genre = _genreBox.Text?.Trim();
            var showtimesText = _showtimesBox.Text?.Trim();
            var duration = _durationBox.Text?.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isVerified)
            {
                MessageBox.Show("Please verify the movie details before saving.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var showtimes = new string[] { };
            if (!string.IsNullOrEmpty(showtimesText))
            {
                showtimes = showtimesText.Split(',');
                for (int i = 0; i < showtimes.Length; i++) showtimes[i] = showtimes[i].Trim();
            }

            CreatedMovie = new MovieItem { Title = title, Genre = genre ?? string.Empty, Showtimes = showtimes, ImagePath = _imagePath, Duration = duration ?? string.Empty };
            this.DialogResult = true;
            this.Close();
        }
    }
}
