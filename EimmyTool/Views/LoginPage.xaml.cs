using EimmyTool.Infrastructure;
using EimmyTool.Security;
using EimmyTool.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views
{
    public sealed partial class LoginPage : Page
    {
        private readonly AuthService _authService;

        public LoginPage()
        {
            this.InitializeComponent();

            var service = new AuthService(DatabaseConfig.ConnectionString); // adjust if needed
            //_authService = new AuthService(service);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter username and password.");
                return;
            }

            var hash = PasswordHasher.Hash(password);

            if (_authService.ValidateUser(username, hash))
            {
                Frame.Navigate(typeof(MainWindow));
            }
            else
            {
                ShowError("Invalid credentials or inactive user.");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
