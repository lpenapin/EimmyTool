using EimmyTool.Infrastructure;
using EimmyTool.Models;
using EimmyTool.Security;
using EimmyTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool
{
    public sealed partial class MainWindow : Window
    {
        private readonly UserService _authUser;

        public MainWindow()
        {
            this.InitializeComponent();
            _authUser = new UserService(DatabaseConfig.ConnectionString);
            LoginOverlay.Visibility = Visibility.Visible;
            RootFrame.Navigate(typeof(Views.HomePage));
            System.Diagnostics.Debug.WriteLine("DATABASE LOCATION: " + DatabaseConfig.DbPath);
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UserIdInput.Text;
            string password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                LoginErrorMessage.Visibility = Visibility.Visible;
                LoginErrorMessage.Text = "Por favor, ingrese usuario y contraseña";
                return;
            }
            string inputHash = User.HashPassword(PasswordInput.Password);
            System.Diagnostics.Debug.WriteLine($"HASH GENERADO: {inputHash}");

            var authenticatedUser = _authUser.AuthenticateAndGetUser(username, inputHash); // Modified to return user object

            if (authenticatedUser != null)
            {
                if (authenticatedUser.ResetPassword == 1)
                {
                    LoginOverlay.Visibility = Visibility.Collapsed;
                    ResetPasswordOverlay.Visibility = Visibility.Visible;
                }
                else
                {
                    // Standard login flow
                    CompleteLogin();
                }
            }
            else
            {
                LoginErrorMessage.Visibility = Visibility.Visible;
                LoginErrorMessage.Text = "Usuario o clave inválida.";
            }
        }
        private void ApplyPermissions()
        {
            if (User.CurrentUser == null) return;

            bool isAdmin = User.CurrentUser._isAdmin;

            foreach (var item in NavView.MenuItems.OfType<NavigationViewItem>())
            {
                // Example: Only Admins see Reports and Inventory
                if (item.Tag?.ToString() == "Reports" || item.Tag?.ToString() == "Inventory")
                {
                    item.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        private void UpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPass = NewPasswordInput.Password;
            string confirmPass = ConfirmPasswordInput.Password;

            if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 6)
            {
                ResetErrorMessage.Text = "La clave debe tener al menos 6 caracteres.";
                ResetErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (newPass != confirmPass)
            {
                ResetErrorMessage.Text = "Las contraseñas no coinciden.";
                ResetErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            // 1. Hash the new password
            string hashedPass = User.HashPassword(newPass);

            // 2. Update Database: SET password = hashedPass, reset_password = 0 WHERE id = _pendingResetUserId
            bool success = _authUser.UpdateUserPassword(User.CurrentUser.Id, hashedPass, 0);

            if (success)
            {
                ResetPasswordOverlay.Visibility = Visibility.Collapsed;
                CompleteLogin();
            }
        }
        private void CompleteLogin()
        {
            ApplyPermissions();
            LoginOverlay.Visibility = Visibility.Collapsed;
            NavView.Visibility = Visibility.Visible;
            RootFrame.Navigate(typeof(Views.HomePage));
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Limpieza básica de seguridad antes de salir
            User.CurrentUser = null;

            // Cerrar la aplicación de forma inmediata
            Application.Current.Exit();
        }
        private void NavView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Home":
                        RootFrame.Navigate(typeof(Views.HomePage));
                        break;

                    case "Selling":
                        RootFrame.Navigate(typeof(Views.SellingPage));
                        break;

                    case "Buying":
                        RootFrame.Navigate(typeof(Views.BuyPage));
                        break;

                    case "Inventory":
                        RootFrame.Navigate(typeof(Views.InventoryPage));
                        break;

                    case "Reports":
                        RootFrame.Navigate(typeof(Views.ReportsPage));
                        break;

                    case "Contacts":
                        RootFrame.Navigate(typeof(Views.ContactsPage));
                        break;

                    case "Returns":
                        RootFrame.Navigate(typeof(Views.ReturnPage));
                        break;

                    case "SuppliersReturns":
                        RootFrame.Navigate(typeof(Views.ReturnSuppliersPage));
                        break;

                    case "GeneralExpenses":
                        RootFrame.Navigate(typeof(Views.GeneralExpensesPage));
                        break;

                    case "About":
                        RootFrame.Navigate(typeof(Views.AboutPage));
                        break;
                }
            }
        }
    }
}
