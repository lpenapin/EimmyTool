using EimmyTool.Infrastructure;
using EimmyTool.Models;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;

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
        public ObservableCollection<Movement> Movements { get; set; } = new();
        public ObservableCollection<Client> _selectedClient { get; set; } = new();
        public ObservableCollection<User> _selectedUser { get; set; } = new();

        private readonly ClientService _clientService;
        private readonly ProviderService _providerService;
        private readonly UserService _userService;
        private readonly MovementService _movementService;

        public ContactsPage()
        {
            this.InitializeComponent();

            _clientService = new ClientService(DatabaseConfig.ConnectionString);
            _providerService = new ProviderService(DatabaseConfig.ConnectionString);
            _userService = new UserService(DatabaseConfig.ConnectionString);
            _movementService = new MovementService(DatabaseConfig.ConnectionString);

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
        {
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
        private async void EditClient_Click(object sender, RoutedEventArgs e) 
        {
            var client = _selectedClient[0];
            var txtDni = new TextBox { Header = "Cédula/NIT",Text = client.DNI, Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre Completo",Text = client.Name, Margin = new Thickness(0, 0, 0, 10) };
            var txtEmail = new TextBox { Header = "Email",Text = client.Email, Margin = new Thickness(0, 0, 0, 10) };
            var txtPhone = new TextBox { Header = "Teléfono",Text = client.Phone, Margin = new Thickness(0, 0, 0, 10) };
            var txtAddress = new TextBox { Header = "Dirección",Text = client.Address, Margin = new Thickness(0, 0, 0, 10) };

            var panel = new StackPanel();
            panel.Children.Add(txtDni);
            panel.Children.Add(txtName);
            panel.Children.Add(txtEmail);
            panel.Children.Add(txtPhone);
            panel.Children.Add(txtAddress);

            var dialog = new ContentDialog
            {
                Title = "Editar Cliente",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                client.DNI = txtDni.Text;
                client.Name = txtName.Text;
                client.Email = txtEmail.Text;
                client.Phone = txtPhone.Text;
                client.Address = txtAddress.Text;
                try
                {
                    UpdateClient(client);
                }
                catch (Exception ex)
                {
                    // Always wrap DB calls in try-catch to handle locked files or syntax errors
                    Debug.WriteLine($"Database error: {ex.Message}");
                }
                LoadData();
            }
        }
        private void UpdateClient(Client client)
        {
            using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    UPDATE Clients 
                    SET name = @name,
                        email = @email,
                        phone = @phone,
                        address = @address,
                        DNI = @dni
                    WHERE id = @client
                """;
                cmd.Parameters.AddWithValue("@name", client.Name);
                cmd.Parameters.AddWithValue("@email", client.Email);
                cmd.Parameters.AddWithValue("@phone", client.Phone);
                cmd.Parameters.AddWithValue("@address", client.Address);
                cmd.Parameters.AddWithValue("@dni", client.DNI);
                cmd.Parameters.AddWithValue("@client", client.Id);
                cmd.ExecuteNonQuery();
            }
            
        }
        private async void EditSupplier_Click(object sender, RoutedEventArgs e) 
        {
            var supplier = _selectedClient[0];
            var txtDni = new TextBox { Header = "Cédula/NIT", Text = supplier.DNI, Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre Completo", Text = supplier.Name, Margin = new Thickness(0, 0, 0, 10) };
            var txtEmail = new TextBox { Header = "Email", Text = supplier.Email, Margin = new Thickness(0, 0, 0, 10) };
            var txtPhone = new TextBox { Header = "Teléfono", Text = supplier.Phone, Margin = new Thickness(0, 0, 0, 10) };
            var txtAddress = new TextBox { Header = "Dirección", Text = supplier.Address, Margin = new Thickness(0, 0, 0, 10) };

            var panel = new StackPanel();
            panel.Children.Add(txtDni);
            panel.Children.Add(txtName);
            panel.Children.Add(txtEmail);
            panel.Children.Add(txtPhone);
            panel.Children.Add(txtAddress);

            var dialog = new ContentDialog
            {
                Title = "Editar Proveedor",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                supplier.DNI = txtDni.Text;
                supplier.Name = txtName.Text;
                supplier.Email = txtEmail.Text;
                supplier.Phone = txtPhone.Text;
                supplier.Address = txtAddress.Text;
                try
                {
                    UpdateSupplier(supplier);
                }
                catch (Exception ex)
                {
                    // Always wrap DB calls in try-catch to handle locked files or syntax errors
                    Debug.WriteLine($"Database error: {ex.Message}");
                }
                LoadData();
            }
        }
        private void UpdateSupplier(Client supplier)
        {
            using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    UPDATE Suppliers 
                    SET name = @name,
                        email = @email,
                        phone = @phone,
                        address = @address,
                        DNI = @dni
                    WHERE id = @client
                """;
                cmd.Parameters.AddWithValue("@name", supplier.Name);
                cmd.Parameters.AddWithValue("@email", supplier.Email);
                cmd.Parameters.AddWithValue("@phone", supplier.Phone);
                cmd.Parameters.AddWithValue("@address", supplier.Address);
                cmd.Parameters.AddWithValue("@dni", supplier.DNI);
                cmd.Parameters.AddWithValue("@client", supplier.Id);
                cmd.ExecuteNonQuery();
            }

        }
        private async void EditUser_Click(object sender, RoutedEventArgs e) 
        {
            var user = _selectedUser[0];
            var txtUserName = new TextBox { Header = "Usuario", Text = user.Username, Margin = new Thickness(0, 0, 0, 10) };
            var txtDni = new TextBox { Header = "Cédula", Text = user.DNI, Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre Completo", Text = user.Name, Margin = new Thickness(0, 0, 0, 10) };
            var txtEmail = new TextBox { Header = "Email", Text = user.Email, Margin = new Thickness(0, 0, 0, 10) };
            var txtPhone = new TextBox { Header = "Teléfono", Text = user.Phone, Margin = new Thickness(0, 0, 0, 10) };
            var txtAddress = new TextBox { Header = "Dirección", Text = user.Address, Margin = new Thickness(0, 0, 0, 10) };

            var panel = new StackPanel();
            panel.Children.Add(txtUserName);
            panel.Children.Add(txtDni);
            panel.Children.Add(txtName);
            panel.Children.Add(txtEmail);
            panel.Children.Add(txtPhone);
            panel.Children.Add(txtAddress);
            

            var dialog = new ContentDialog
            {
                Title = "Editar Usuario",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                user.DNI = txtDni.Text;
                user.Name = txtName.Text;
                user.Email = txtEmail.Text;
                user.Phone = txtPhone.Text;
                user.Address = txtAddress.Text;
                user.Username = txtUserName.Text;
                try
                {
                    UpdateUser(user);
                }
                catch (Exception ex)
                {
                    // Always wrap DB calls in try-catch to handle locked files or syntax errors
                    Debug.WriteLine($"Database error: {ex.Message}");
                }
                LoadData();
            }
        }
        private void UpdateUser(User user)
        {
            using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    UPDATE Users  
                    SET name = @name,
                        email = @email,
                        phone = @phone,
                        address = @address,
                        DNI = @dni,
                        user_name = @username
                    WHERE id = @user
                """;
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@phone", user.Phone);
                cmd.Parameters.AddWithValue("@address", user.Address);
                cmd.Parameters.AddWithValue("@dni", user.DNI);
                cmd.Parameters.AddWithValue("@user", user.Id);
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.ExecuteNonQuery();
            }

        }
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
        private void ClientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsList.SelectedItem is Client selectedClient)
            {
                BtnEditClient.IsEnabled = true;
                BtnHistory.IsEnabled = true;
                // Only enable "Pay Debt" if they actually owe money
                BtnPayDebt.IsEnabled = selectedClient.Debt > 0;
                //var existing = _selectedClient.FirstOrDefault(i => i.Id == selectedClient.Id);
                _selectedClient.Clear();
                _selectedClient.Add(new Client
                {
                    Id = selectedClient.Id,
                    DNI = selectedClient.DNI,
                    Name = selectedClient.Name,
                    Address = selectedClient.Address,
                    Phone = selectedClient.Phone,
                    Email = selectedClient.Email,
                    Debt = selectedClient.Debt
                });
            }
            else
            {
                // Disable buttons if nothing is selected
                BtnEditClient.IsEnabled = false;
                BtnHistory.IsEnabled = false;
                BtnPayDebt.IsEnabled = false;
            }
        }
        private void SuppliersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppliersList.SelectedItem is Client selectedSupplier)
            {
                BtnEditSupplier.IsEnabled = true;
                BtnHistorySupplier.IsEnabled = true;
                // Only enable "Pay Debt" if they actually owe money
                BtnPayDebtSupplier.IsEnabled = selectedSupplier.Debt > 0;
                //var existing = _selectedClient.FirstOrDefault(i => i.Id == selectedClient.Id);
                _selectedClient.Clear();
                _selectedClient.Add(new Client
                {
                    Id = selectedSupplier.Id,
                    DNI = selectedSupplier.DNI,
                    Name = selectedSupplier.Name,
                    Address = selectedSupplier.Address,
                    Phone = selectedSupplier.Phone,
                    Email = selectedSupplier.Email,
                    Debt = selectedSupplier.Debt
                });
            }
            else
            {
                // Disable buttons if nothing is selected
                BtnEditSupplier.IsEnabled = false;
                BtnHistorySupplier.IsEnabled = false;
                BtnPayDebtSupplier.IsEnabled = false;
            }
        }
        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is User selectedUser)
            {
                BtnEditUser.IsEnabled = true;
                BtnHistoryUser.IsEnabled = true;
                BtnRolUser.IsEnabled = true; //Change later, if current user is admin enable
                BtnStatusUser.IsEnabled = true; //Change later, if current user is admin enable
                BtnPayUserSalary.IsEnabled = true; //Change later, if current user is admin enable
                _selectedUser.Clear();
                _selectedUser.Add(new User
                {
                    Id = selectedUser.Id,
                    DNI = selectedUser.DNI,
                    Name = selectedUser.Name,
                    Address = selectedUser.Address,
                    Phone = selectedUser.Phone,
                    Email = selectedUser.Email,
                    IsAdmin = selectedUser.IsAdmin,
                    IsActive = selectedUser.IsActive,
                    Username = selectedUser.Username
                });
            }
            else
            {
                // Disable buttons if nothing is selected
                BtnEditUser.IsEnabled = false;
                BtnHistoryUser.IsEnabled = false;
                BtnRolUser.IsEnabled = false;
                BtnStatusUser.IsEnabled = false;
                BtnPayUserSalary.IsEnabled = false;
            }
        }
        private async void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            var client = _selectedClient[0];
            Movements.Clear();
            Name.Text = client.Name;
            DNI.Text = client.DNI;
            var history = _movementService.GetClientHistoryFromDb(client.Id);
            MovementsList.ItemsSource =  history ;
            ContactsTabs.SelectedItem = HistoryTab;
        }
        private async void ViewHistorySupplier_Click(object sender, RoutedEventArgs e)
        {
            var supplier = _selectedClient[0];
            Movements.Clear();
            Name.Text = supplier.Name;
            DNI.Text = supplier.DNI;
            var history = _movementService.GetSupplierHistoryFromDb(supplier.Id);
            MovementsList.ItemsSource = history;
            ContactsTabs.SelectedItem = HistoryTab;
        }
        private async void ViewHistoryUser_Click(object sender, RoutedEventArgs e)
        {
            var user = _selectedUser[0];
            Movements.Clear();
            Name.Text = user.Name;
            DNI.Text = user.DNI;
            var history = _movementService.GetUserHistoryFromDb(user.Id);
            MovementsList.ItemsSource = history;
            ContactsTabs.SelectedItem = HistoryTab;
        }
        private async void PayDebt_Click(object sender, RoutedEventArgs e)
        {
            var client = _selectedClient[0];
            var dialog = new DebtPaymentDialog(client);
            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                decimal amountToPay = dialog.PaymentAmount;

                if (amountToPay > 0)
                {
                    using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try {
                        var invoiceId = InsertInvoice(conn, tx, client.Id, amountToPay);
                        InsertCashFlowMovement(conn, tx, invoiceId, amountToPay);
                        UpdateClientDebt(conn, tx, client.Id, amountToPay);
                        tx.Commit();
                        decimal saldo = client.Debt - amountToPay;
                        await ShowReceiptDialog(invoiceId.ToString(), client.DNI, client.Name, saldo, amountToPay);
                        LoadData();
                    }                    
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        await new ContentDialog
                        {
                            Title = "Error",
                            Content = ex.Message,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        }.ShowAsync();
                    }
                    // Execute your logic here (e.g., API call or DB update)
                    //PerformPayment(client.Id, amountToPay);
                }
            }

        }
        private int InsertInvoice(SqliteConnection conn, SqliteTransaction tx, int clientId, decimal abono)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO PayDebtInvoices (client_id, user_id, paid)
                VALUES (@clientId, @userId, @paid);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@userId", 1); // TODO: replace with logged user
            cmd.Parameters.AddWithValue("@paid", abono);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private void InsertCashFlowMovement(SqliteConnection conn, SqliteTransaction tx, int invoiceId, decimal abono)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO CashFlow
                (type, detail, value, reference_table, reference_id, user_id)
                VALUES ('DEBITO', 'Abono deuda cliente', @value, 'PayDebtInvoices', @ref_id, @user)
            """;
            cmd.Parameters.AddWithValue("@user", 1); //update later
            cmd.Parameters.AddWithValue("@value", abono);
            cmd.Parameters.AddWithValue("@ref_id", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private void UpdateClientDebt(SqliteConnection conn, SqliteTransaction tx, int clientId, decimal debt)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Clients 
                SET debt = debt - @debt
                WHERE id = @client
            """;

            cmd.Parameters.AddWithValue("@debt", debt);
            cmd.Parameters.AddWithValue("@client", clientId);

            cmd.ExecuteNonQuery();
        }
        private async Task ShowReceiptDialog(string invoiceNo, string dni, string name, decimal saldoTotal, decimal abono)
        {
            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = $"Factura N°: {invoiceNo}", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Cliente/Proveedor: {name} ({dni})" });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray), X2 = 300, StrokeThickness = 1 });
            stack.Children.Add(new TextBlock { Text = $"Pago: {abono:C2}", HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold });

            if (saldoTotal > 0)
            {
                stack.Children.Add(new TextBlock { Text = $"Saldo Pendiente: {saldoTotal:C2}", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red), HorizontalAlignment = HorizontalAlignment.Right });
            }

            var dialog = new ContentDialog
            {
                Title = "Abono deuda",
                Content = stack,
                PrimaryButtonText = "Imprimir",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            /*if (result == ContentDialogResult.Primary)
            {
                // Call your printing logic here
                //PrintInvoice(invoiceNo, products, total, debt);
            }*/
        }
        private async void PayDebtSupplier_Click(object sender, RoutedEventArgs e) 
        {
            var supplier = _selectedClient[0];
            var dialog = new DebtPaymentDialog(supplier);
            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                decimal amountToPay = dialog.PaymentAmount;

                if (amountToPay > 0)
                {
                    using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try
                    {
                        var invoiceId = InsertSupplierInvoice(conn, tx, supplier.Id, amountToPay);
                        InsertSupplierCashFlowMovement(conn, tx, invoiceId, amountToPay);
                        UpdateSupplierDebt(conn, tx, supplier.Id, amountToPay);
                        tx.Commit();
                        decimal saldo = supplier.Debt - amountToPay;
                        await ShowReceiptDialog(invoiceId.ToString(), supplier.DNI, supplier.Name, saldo, amountToPay);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        await new ContentDialog
                        {
                            Title = "Error",
                            Content = ex.Message,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        }.ShowAsync();
                    }
                }
            }
        }
        private int InsertSupplierInvoice(SqliteConnection conn, SqliteTransaction tx, int supplierId, decimal abono)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO PayCreditInvoices (supplier_id, user_id, paid)
                VALUES (@supplierId, @userId, @paid);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@supplierId", supplierId);
            cmd.Parameters.AddWithValue("@userId", 1); // TODO: replace with logged user
            cmd.Parameters.AddWithValue("@paid", abono);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private void InsertSupplierCashFlowMovement(SqliteConnection conn, SqliteTransaction tx, int invoiceId, decimal abono)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO CashFlow
                (type, detail, value, reference_table, reference_id, user_id)
                VALUES ('CREDITO', 'Abono deuda proveedor', @value, 'PayCreditInvoices ', @ref_id, @user)
            """;
            cmd.Parameters.AddWithValue("@user", 1); //update later
            cmd.Parameters.AddWithValue("@value", abono);
            cmd.Parameters.AddWithValue("@ref_id", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private void UpdateSupplierDebt(SqliteConnection conn, SqliteTransaction tx, int suppliertId, decimal debt)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Suppliers 
                SET debt = debt - @debt
                WHERE id = @supplier
            """;

            cmd.Parameters.AddWithValue("@debt", debt);
            cmd.Parameters.AddWithValue("@supplier", suppliertId);

            cmd.ExecuteNonQuery();
        }
        private async void RolChangeUser_Click(object sender, RoutedEventArgs e) 
        {
            var user = _selectedUser[0];
            var rol = 1;
            if (user.IsAdmin == 1) { rol = 0; }
            using var connection = new SqliteConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Users 
                SET is_admin = @rol
                WHERE id = @user
            """;

            cmd.Parameters.AddWithValue("@rol", rol);
            cmd.Parameters.AddWithValue("@user", user.Id);

            cmd.ExecuteNonQuery();
            LoadData();
        }
        private async void StatusChangeUser_Click(object sender, RoutedEventArgs e) 
        {
            var user = _selectedUser[0];
            var status = 1;
            if (user.IsActive == 1) { status = 0; }
            using var connection = new SqliteConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Users 
                SET is_active = @status
                WHERE id = @user
            """;

            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@user", user.Id);

            cmd.ExecuteNonQuery();
            LoadData();
        }
        private async void PayUserSalary_Click(object sender, RoutedEventArgs e) 
        {
            var user = _selectedUser[0];
            var dialog = new SalaryPaymentDialog(user);
            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                decimal amountToPay = dialog.TotalSalary;

                if (amountToPay > 0)
                {
                    using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
                    conn.Open();
                    using var tx = conn.BeginTransaction();
                    try
                    {
                        var invoiceId = InsertSalaryInvoice(conn, tx, user.Id, amountToPay, dialog.HoursWorked, dialog.CostPerHour);
                        InsertSalaryCashFlowMovement(conn, tx, invoiceId, amountToPay);
                        tx.Commit();
                        await ShowPaymentDialog(invoiceId.ToString(), user.DNI, user.Name, dialog.HoursWorked, dialog.CostPerHour, amountToPay);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        await new ContentDialog
                        {
                            Title = "Error",
                            Content = ex.Message,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        }.ShowAsync();
                    }
                }
            }
        }
        private int InsertSalaryInvoice(SqliteConnection conn, SqliteTransaction tx, int userId, decimal paid, double HoursW, double HoursC)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO Salaries (user_id, hours_worked, hour_cost, total_paid)
                VALUES (@userId, @hoursW, @hoursC, @paid);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@paid", paid);
            cmd.Parameters.AddWithValue("@hoursW", HoursW);
            cmd.Parameters.AddWithValue("@hoursC", HoursC);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private void InsertSalaryCashFlowMovement(SqliteConnection conn, SqliteTransaction tx, int invoiceId, decimal abono)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO CashFlow
                (type, detail, value, reference_table, reference_id, user_id)
                VALUES ('CREDITO', 'Pago Salario', @value, 'Salaries', @ref_id, @user)
            """;
            cmd.Parameters.AddWithValue("@user", 1); //update later
            cmd.Parameters.AddWithValue("@value", abono);
            cmd.Parameters.AddWithValue("@ref_id", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private async Task ShowPaymentDialog(string invoiceNo, string dni, string name, double hoursW, double hoursC, decimal paid)
        {
            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = $"Factura N°: {invoiceNo}", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Empleado: {name} ({dni})" });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray), X2 = 300, StrokeThickness = 1 });
            stack.Children.Add(new TextBlock { Text = $"Horas Trabajadas: {hoursW:N2}", HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            stack.Children.Add(new TextBlock { Text = $"Valor Hora: {hoursC:C2}", HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            stack.Children.Add(new TextBlock { Text = $"Total Pagado: {paid:C2}", HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold });


            var dialog = new ContentDialog
            {
                Title = "Pago Salario",
                Content = stack,
                PrimaryButtonText = "Imprimir",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            /*if (result == ContentDialogResult.Primary)
            {
                // Call your printing logic here
                //PrintInvoice(invoiceNo, products, total, debt);
            }*/
        }
    }
}
