using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MovieBookingWPF.Models; // Imported to access AppDbContext and MovieItem models

namespace MovieBookingWPF
{
    public class AddMovieWindow : Window
    {
        private TextBox _titleBox;
        private TextBox _genreBox;
        private TextBox _durationBox;
        private TextBox _showtimesBox;
        private Image _previewImage;
        private string _imagePath = string.Empty;
        private Button _verifyBtn;
        private bool _isVerified = false;
        public MovieItem CreatedMovie { get; private set; }

        public AddMovieWindow()
        {
            Title = "Add New Movie";
            Width = 420;
            Height = 440;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.Transparent;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;

            var root = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Children.Add(new TextBlock { Text = "Title", FontWeight = FontWeights.SemiBold });
            _titleBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(_titleBox);

            stack.Children.Add(new TextBlock { Text = "Genre", FontWeight = FontWeights.SemiBold });
            _genreBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(_genreBox);

            stack.Children.Add(new TextBlock { Text = "Duration (e.g. 2h 10m)", FontWeight = FontWeights.SemiBold });
            _durationBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(_durationBox);

            stack.Children.Add(new TextBlock { Text = "Showtimes (comma separated)", FontWeight = FontWeights.SemiBold });
            _showtimesBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(_showtimesBox);

            // Image picker
            stack.Children.Add(new TextBlock { Text = "Poster image (optional)", FontWeight = FontWeights.SemiBold });
            var imgRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 12) };
            _previewImage = new Image { Width = 48, Height = 64, Margin = new Thickness(0, 0, 12, 0), Stretch = Stretch.UniformToFill };
            var chooseBtn = new Button { Content = "Choose image...", Padding = new Thickness(10, 4, 10, 4) };
            chooseBtn.Click += (s, e) =>
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Select Poster Image",
                    Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                    Multiselect = false
                };
                if (dlg.ShowDialog() == true)
                {
                    _imagePath = dlg.FileName;
                    try
                    {
                        _previewImage.Source = new BitmapImage(new Uri(_imagePath));
                    }
                    catch { }
                }
            };
            imgRow.Children.Add(_previewImage);
            imgRow.Children.Add(chooseBtn);
            stack.Children.Add(imgRow);

            // Button Control Panel Setup Row
            var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 8, 0, 0) };

            // A. VERIFY BUTTON
            Button addBtn = null;
            _verifyBtn = new Button { Content = "Verify", Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(12, 6, 12, 6), Background = new SolidColorBrush(Color.FromRgb(240, 243, 247)), Foreground = Brushes.Black, BorderThickness = new Thickness(0) };
            _verifyBtn.Click += (s, e) => {
                var title = _titleBox.Text?.Trim();
                var duration = _durationBox.Text?.Trim();
                if (string.IsNullOrEmpty(title)) { MessageBox.Show("Please enter a title before verifying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                if (string.IsNullOrEmpty(duration)) { MessageBox.Show("Please enter duration before verifying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

                _isVerified = true;
                _verifyBtn.IsEnabled = false;
                _verifyBtn.Background = new SolidColorBrush(Color.FromRgb(34, 197, 94));
                _verifyBtn.Foreground = Brushes.White;

                if (addBtn != null) addBtn.IsEnabled = true;
            };

            // B. CANCEL BUTTON
            var cancelBtn = new Button { Content = "Cancel", Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(12, 6, 12, 6) };
            cancelBtn.Click += (s, e) => {
                this.DialogResult = false;
                this.Close();
            };

            // C. ADD MOVIE BUTTON
            addBtn = new Button { Content = "Add Movie", Background = new SolidColorBrush(Color.FromRgb(17, 41, 51)), Foreground = Brushes.White, Padding = new Thickness(12, 6, 12, 6), IsEnabled = false, BorderThickness = new Thickness(0) };
            addBtn.Click += Add_Click;

            row.Children.Add(_verifyBtn);
            row.Children.Add(cancelBtn);
            row.Children.Add(addBtn);
            stack.Children.Add(row);

            root.Child = stack;
            Content = root;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
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

            // Cleanly create item and pipe string text directly into your database conversion property wrapper 
            CreatedMovie = new MovieItem
            {
                Title = title,
                Genre = genre ?? string.Empty,
                ShowtimesDatabase = showtimesText ?? string.Empty, // This automatically triggers split parsing and sets Showtimes safely!
                ImagePath = _imagePath ?? string.Empty,
                Duration = duration ?? string.Empty,
                Price = 10.00m
            };

            // Pushes record changes straight to LocalDB
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Movies.Add(CreatedMovie);
                    db.SaveChanges();
                }
                MessageBox.Show("Movie successfully added to local database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}