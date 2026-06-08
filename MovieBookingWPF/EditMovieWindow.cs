using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MovieBookingWPF.Models;

namespace MovieBookingWPF
{
    public class EditMovieWindow : Window
    {
        private readonly MovieItem _targetMovie;
        private TextBox _titleBox;
        private TextBox _genreBox;
        private TextBox _durationBox;
        private TextBox _showtimesBox;
        private Image _previewImage;
        private string? _imagePath;
        private Button _applyBtn;
        private Button _verifyBtn;
        private bool _isVerified = false;

        public EditMovieWindow(MovieItem movie)
        {
            _targetMovie = movie ?? throw new ArgumentNullException(nameof(movie)); 

            Title = "Edit Movie";
            Width = 420;
            Height = 360;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.Transparent;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;

            var root = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12)
            };

            // header with title and top-right close button (red circle X)
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerTitle = new TextBlock
            {
                Text = "Edit movie",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(headerTitle, 0);
            headerGrid.Children.Add(headerTitle);

            var closeBtn = new Button
            {
                Content = "X",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                ToolTip = "Close"
            };
            closeBtn.Click += (s, e) => { this.DialogResult = false; this.Close(); };

            try
            {
                var closeXaml = @"
<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>
  <Border CornerRadius='16' Background='{TemplateBinding Background}' Width='{TemplateBinding Width}' Height='{TemplateBinding Height}'>
    <TextBlock Text='{TemplateBinding Content}' Foreground='{TemplateBinding Foreground}' HorizontalAlignment='Center' VerticalAlignment='Center' FontWeight='Bold'/>
  </Border>
</ControlTemplate>";
                closeBtn.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(closeXaml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to parse close button template: " + ex);
            }

            Grid.SetColumn(closeBtn, 1);
            headerGrid.Children.Add(closeBtn);

            var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 8, 0, 0) };
            stack.Children.Add(headerGrid);

            // Title
            stack.Children.Add(new TextBlock { Text = "Title", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,8,0,4) });
            _titleBox = new TextBox { Text = _targetMovie.Title ?? string.Empty, Margin = new Thickness(0,0,0,8) };
            stack.Children.Add(_titleBox);  

            // Genre
            stack.Children.Add(new TextBlock { Text = "Genre", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,4) });
            _genreBox = new TextBox { Text = _targetMovie.Genre ?? string.Empty, Margin = new Thickness(0,0,0,8) };
            stack.Children.Add(_genreBox);

            // Duration
            stack.Children.Add(new TextBlock { Text = "Duration (e.g. 2h 10m)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,4) });
            _durationBox = new TextBox { Text = _targetMovie.Duration ?? string.Empty, Margin = new Thickness(0,0,0,8) };
            stack.Children.Add(_durationBox);

            // Showtimes
            stack.Children.Add(new TextBlock { Text = "Showtimes (comma separated)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,4) });
            _showtimesBox = new TextBox { Text = (_targetMovie.Showtimes != null) ? string.Join(", ", _targetMovie.Showtimes) : string.Empty, Margin = new Thickness(0,0,0,8) };
            stack.Children.Add(_showtimesBox);

            // Image picker row
            stack.Children.Add(new TextBlock { Text = "Poster image (optional)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,4) });
            var imgRow = new StackPanel { Orientation = Orientation.Horizontal };
            _previewImage = new Image { Width = 64, Height = 96, Margin = new Thickness(0,6,12,12) };
            if (!string.IsNullOrEmpty(_targetMovie.ImagePath) && File.Exists(_targetMovie.ImagePath))
            {
                try { _previewImage.Source = new BitmapImage(new Uri(_targetMovie.ImagePath)); _imagePath = _targetMovie.ImagePath; }
                catch { /* ignore preview load errors */ }
            }
            var chooseBtn = new Button { Content = "Choose image...", Padding = new Thickness(10,6,10,6) };
            chooseBtn.Click += (s, e) =>
            {
                var dlg = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*" };
                if (dlg.ShowDialog() == true)
                {
                    _imagePath = dlg.FileName;
                    try { _previewImage.Source = new BitmapImage(new Uri(_imagePath)); }
                    catch { }
                }
            };
            imgRow.Children.Add(_previewImage);
            imgRow.Children.Add(chooseBtn);
            stack.Children.Add(imgRow);

            // action row: Verify, Cancel, Apply
            var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,12,0,0) };

            // create apply button first so handlers can safely reference it
            _applyBtn = new Button { Content = "Apply", Background = new SolidColorBrush(Color.FromRgb(17,41,51)), Foreground = Brushes.White, Padding = new Thickness(12,6,12,6), IsEnabled = false };
            _applyBtn.Click += Apply_Click;

            _verifyBtn = new Button { Content = "Verify", Margin = new Thickness(0,0,8,0), Padding = new Thickness(12,6,12,6), Background = new SolidColorBrush(Color.FromRgb(240,243,247)), Foreground = Brushes.Black };
            _verifyBtn.Click += (s, e) =>
            {
                var title = _titleBox.Text?.Trim();
                var duration = _durationBox.Text?.Trim();
                if (string.IsNullOrEmpty(title)) { MessageBox.Show("Please enter a title before verifying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                if (string.IsNullOrEmpty(duration)) { MessageBox.Show("Please enter duration before verifying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                _isVerified = true;
                _verifyBtn.IsEnabled = false;
                _verifyBtn.Background = new SolidColorBrush(Color.FromRgb(34,197,94));
                _verifyBtn.Foreground = Brushes.White;
                _applyBtn.IsEnabled = true;
            };

            var cancelBtn = new Button { Content = "Cancel", Margin = new Thickness(0,0,8,0), Padding = new Thickness(12,6,12,6) };
            cancelBtn.Click += (s, e) => { this.DialogResult = false; this.Close(); };

            row.Children.Add(_verifyBtn);
            row.Children.Add(cancelBtn);
            row.Children.Add(_applyBtn);
            stack.Children.Add(row);

            root.Child = stack;
            Content = root;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            var title = _titleBox.Text?.Trim();
            var genre = _genreBox.Text?.Trim();
            var duration = _durationBox.Text?.Trim();
            var showtimesText = _showtimesBox.Text?.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isVerified)
            {
                MessageBox.Show("Please verify the movie details before applying.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var dbMovie = db.Movies.FirstOrDefault(x => x.Id == _targetMovie.Id);
                    if (dbMovie == null)
                    {
                        MessageBox.Show("Movie not found in database.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    dbMovie.Title = title;
                    dbMovie.Genre = genre ?? string.Empty;
                    dbMovie.Duration = duration ?? string.Empty;
                    dbMovie.ShowtimesDatabase = showtimesText ?? string.Empty;
                    if (!string.IsNullOrEmpty(_imagePath)) dbMovie.ImagePath = _imagePath;

                    db.SaveChanges();
                }

                _targetMovie.Title = title;
                _targetMovie.Genre = genre ?? string.Empty;
                _targetMovie.Duration = duration ?? string.Empty;
                _targetMovie.ShowtimesDatabase = showtimesText ?? string.Empty;
                if (!string.IsNullOrEmpty(_imagePath)) _targetMovie.ImagePath = _imagePath;

                MessageBox.Show("Movie updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
