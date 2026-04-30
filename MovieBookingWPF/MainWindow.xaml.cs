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

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter email and password.", "Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // admin credentials: admin@test.com / 123 -> open admin dashboard
            if (email.Equals("admin@test.com", System.StringComparison.OrdinalIgnoreCase) && password == "123")
            {
                var admin = new AdminDashboardWindow();
                admin.Show();
                this.Close();
                return;
            }

            // otherwise open customer dashboard
            var dashboard = new CustomerDashboardWindow(email);
            dashboard.Show();
            this.Close();
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}