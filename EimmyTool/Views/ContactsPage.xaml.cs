using EimmyTool.Infrastructure;
using EimmyTool.Models;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views
{
    public sealed partial class ContactsPage : Page
    {
        // Usamos ObservableCollection para que la UI se entere de los cambios
        public ObservableCollection<Client> Clients { get; set; } = new();
        public ObservableCollection<Client> Suppliers { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();

        private readonly ClientService _clientService;
        private readonly ProviderService _providerService;
        private readonly UserService _userService;

        public ContactsPage()
        {
            this.InitializeComponent();

            _clientService = new ClientService(DatabaseConfig.ConnectionString);
            _providerService = new ProviderService(DatabaseConfig.ConnectionString);
            _userService = new UserService(DatabaseConfig.ConnectionString);

            LoadData();
        }
        private void LoadData()
        {
            // Limpiamos y cargamos
            Clients.Clear();
            _clientService.GetAll().ForEach(c => Clients.Add(c));
            ClientsList.ItemsSource = Clients;

            Suppliers.Clear();
            _providerService.GetAll().ForEach(p => Suppliers.Add(p));
            SuppliersList.ItemsSource = Suppliers;

            Users.Clear();
            _userService.GetAll().ForEach(u => Users.Add(u));
            UsersList.ItemsSource = Users;
        }
        private async void AddClient_Click(object sender, RoutedEventArgs e)
        {// Crear los campos del formulario
            var txtDni = new TextBox { Header = "Cédula/NIT", Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre Completo", Margin = new Thickness(0, 0, 0, 10) };
            var txtEmail = new TextBox { Header = "Email", Margin = new Thickness(0, 0, 0, 10) };
            var txtPhone = new TextBox { Header = "Teléfono", Margin = new Thickness(0, 0, 0, 10) };
            var txtAddress = new TextBox { Header = "Dirección", Margin = new Thickness(0, 0, 0, 10) };

            var panel = new StackPanel();
            panel.Children.Add(txtDni);
            panel.Children.Add(txtName);
            panel.Children.Add(txtEmail);
            panel.Children.Add(txtPhone);
            panel.Children.Add(txtAddress);

            var dialog = new ContentDialog
            {
                Title = "Nuevo Cliente",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot // Obligatorio en WinUI 3
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newClient = new Client
                {
                    DNI = txtDni.Text,
                    Name = txtName.Text,
                    Email = txtEmail.Text,
                    Phone = txtPhone.Text,
                    Address = txtAddress.Text,
                };

                _clientService.Insert(newClient);
                LoadData(); // Recarga la lista para mostrar el nuevo cliente
            }
        }
        private async void AddSupplier_Click(object sender, RoutedEventArgs e)
        {// Crear los campos del formulario
            var txtDni = new TextBox { Header = "Cédula/NIT", Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre Completo", Margin = new Thickness(0, 0, 0, 10) };
            var txtEmail = new TextBox { Header = "Email", Margin = new Thickness(0, 0, 0, 10) };
            var txtPhone = new TextBox { Header = "Teléfono", Margin = new Thickness(0, 0, 0, 10) };
            var txtAddress = new TextBox { Header = "Dirección", Margin = new Thickness(0, 0, 0, 10) };

            var panel = new StackPanel();
            panel.Children.Add(txtDni);
            panel.Children.Add(txtName);
            panel.Children.Add(txtEmail);
            panel.Children.Add(txtPhone);
            panel.Children.Add(txtAddress);

            var dialog = new ContentDialog
            {
                Title = "Nuevo Proveedor",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot // Obligatorio en WinUI 3
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newClient = new Client
                {
                    DNI = txtDni.Text,
                    Name = txtName.Text,
                    Email = txtEmail.Text,
                    Phone = txtPhone.Text,
                    Address = txtAddress.Text,
                };

                _providerService.Insert(newClient);
                LoadData(); // Recarga la lista para mostrar el nuevo cliente
            }
        }
        private async void AddUser_Click(object sender, RoutedEventArgs e) 
        {
            var txtUser = new TextBox { Header = "Usuario" };
            var txtName = new TextBox { Header = "Nombre Completo" };
            var txtDNI = new TextBox { Header = "Cédula" };
            var txtEmail = new TextBox { Header = "Correo" };
            var txtPhone = new TextBox { Header = "Teléfono" };
            var txtAddress = new TextBox { Header = "Dirección" };
            //var txtPass = new PasswordBox { Header = "Contraseña" };
            var chkAdmin = new CheckBox { Content = "Es Administrador", Margin = new Thickness(0, 10, 0, 0) };

            var panel = new StackPanel { Children = { txtUser, txtName, txtDNI, txtEmail, txtPhone, txtAddress, chkAdmin } };

            var dialog = new ContentDialog
            {
                Title = "Nuevo Usuario de Sistema",
                Content = panel,
                PrimaryButtonText = "Registrar",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var newUser = new User
                {
                    Username = txtUser.Text,
                    Name = txtName.Text,
                    DNI = txtDNI.Text, 
                    Email = txtEmail.Text,
                    Phone = txtPhone.Text,
                    Address = txtAddress.Text,
                    IsAdmin = chkAdmin.IsChecked == true ? 1 : 0,
                };

                _userService.Insert(newUser);
                LoadData();
            }
        }
        private async void EditClient_Click(object sender, RoutedEventArgs e) { }
        private async void EditSupplier_Click(object sender, RoutedEventArgs e) { }
        private async void EditUser_Click(object sender, RoutedEventArgs e) { }
        private void DniBoxClient_LostFocus(object sender, RoutedEventArgs e) 
        {
            var dni = DniBoxClient.Text?.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                ClientText.Text = "";
                LoadData();
                return;
            }
                

            var client = _clientService.GetByDni(dni);
            if (client != null)
            {
                //Success: Show the name and highlight in list
                ClientText.Text = $"Seleccionado: {client.Name}";
                // Strategy: Filter the list to show ONLY this client
                ClientsList.ItemsSource = new List<Client> { client };
            }
            else
            {
                // Fail: Inform the user
                ClientText.Text = "❌ Cliente no existe";
                ClientsList.ItemsSource = null;
            }

        }
        private void DniBoxClient_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                DniBoxClient_LostFocus(sender, null); // Trigger the search
            }
        }
        private void DniBoxSupplier_LostFocus(object sender, RoutedEventArgs e)
        {
            var dni = DniBoxSupplier.Text?.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                SupplierText.Text = "";
                LoadData();
                return;
            }


            var Supplier = _providerService.GetByDni(dni);
            if (Supplier != null)
            {
                //Success: Show the name and highlight in list
                SupplierText.Text = $"Seleccionado: {Supplier.Name}";
                // Strategy: Filter the list to show ONLY this client
                SuppliersList.ItemsSource = new List<Client> { Supplier };
            }
            else
            {
                // Fail: Inform the user
                SupplierText.Text = "❌ Proveedor no existe";
                SuppliersList.ItemsSource = null;
            }

        }
        private void DniBoxSupplier_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                DniBoxSupplier_LostFocus(sender, null); // Trigger the search
            }
        }
        private void DniBoxUser_LostFocus(object sender, RoutedEventArgs e)
        {
            var dni = DniBoxUser.Text?.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                UserText.Text = "";
                LoadData();
                return;
            }


            var User = _userService.GetByDni(dni);
            if (User != null)
            {
                //Success: Show the name and highlight in list
                UserText.Text = $"Seleccionado: {User.Name}";
                // Strategy: Filter the list to show ONLY this client
                UsersList.ItemsSource = new List<User> { User };
            }
            else
            {
                // Fail: Inform the user
                UserText.Text = "❌ Usuario no existe";
                UsersList.ItemsSource = null;
            }

        }
        private void DniBoxUser_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                DniBoxUser_LostFocus(sender, null); // Trigger the search
            }
        }
        // 1. Handle selection to enable/disable action buttons
        private void ClientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is actually selected
            if (ClientsList.SelectedItem is Client selectedClient)
            {
                // Enable buttons
                BtnEditClient.IsEnabled = true;
                BtnHistory.IsEnabled = true;

                // Only enable "Pay Debt" if they actually owe money
                BtnPayDebt.IsEnabled = selectedClient.Debt > 0;
            }
            else
            {
                // Disable buttons if nothing is selected
                BtnEditClient.IsEnabled = false;
                BtnHistory.IsEnabled = false;
                BtnPayDebt.IsEnabled = false;
            }
        }

        // 2. Handle the "View History" click
        private async void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsList.SelectedItem is Client selectedClient)
            {
                // TODO: Open a ContentDialog or navigate to a new page showing the history
                // Example:
                // var historyDialog = new HistoryDialog(selectedClient.Id);
                // await historyDialog.ShowAsync();

                System.Diagnostics.Debug.WriteLine($"Showing history for: {selectedClient.Name}");
            }
        }

        // 3. Handle the "Pay Debt" click
        private async void PayDebt_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsList.SelectedItem is Client selectedClient)
            {
                // TODO: Open a payment window/dialog passing the client data
                // Example:
                // var paymentDialog = new PaymentDialog(selectedClient);
                // await paymentDialog.ShowAsync();

                System.Diagnostics.Debug.WriteLine($"Processing payment for: {selectedClient.Name}, Debt: {selectedClient.Debt}");
            }
        }
    }
}
