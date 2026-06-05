using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MovieBookingWPF.Models; // Added this to recognize your AppDbContext

namespace MovieBookingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _showingPassword = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowPassword_Toggle(object sender, MouseButtonEventArgs e)
        {
            TogglePasswordVisibility();
        }

        private void TogglePasswordVisibility()
        {
            _showingPassword = !_showingPassword;

            if (_showingPassword)
            {
                // copy password to the plain textbox and show it
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                // copy back and hide the plain textbox
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // keep the plain text box in sync when user types while hidden is off
            if (_showingPassword)
            {
                PasswordTextBox.Text = PasswordBox.Password;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text?.Trim();
            var password = _showingPassword ? PasswordTextBox.Text : PasswordBox.Password;

            // 1. Validation Check
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter email and password.", "Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2. Database Lookup via Entity Framework
            using (var db = new AppDbContext())
            {
                // Find a user matching both the email and the password text
                var authenticatedUser = db.Users
                    .FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.Password == password);

                if (authenticatedUser != null)
                {
                    MessageBox.Show($"Login successful! Welcome back, {authenticatedUser.Email}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 3. Routing System based on Database Role
                    if (authenticatedUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        var admin = new AdminDashboardWindow();
                        admin.Show();
                    }
                    else
                    {
                        var dashboard = new CustomerDashboardWindow(authenticatedUser.Email);
                        dashboard.Show();
                    }

                    this.Close(); // Close the login window safely
                }
                else
                {
                    // If no match was found in the database
                    MessageBox.Show("Invalid Email or Password. Please try again or register.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Leave empty or add placeholder handling if required
        }
    }
}