using EimmyTool.Infrastructure;
using EimmyTool.Models;
using EimmyTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Security.Authentication.OAuth;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views
{
    public sealed partial class ReturnPage : Page
    {
        public ObservableCollection<InvoiceItem> Invoices { get; set; } = new();
        public ObservableCollection<InvoiceItem> ReturnItems { get; set; } = new();
        private readonly ProviderService _providerService;
        private readonly InvoiceService _invoiceService;
        private readonly ClientService _clientService;
        private readonly ProductService _productService;
        //private readonly ObservableCollection<BuyItem> _items = new();
        private decimal _taxRate = 0m; // 0%
        private decimal Subtotal => ReturnItems.Sum(i => i.Total);
        private decimal TaxAmount => Subtotal * _taxRate / 100m;
        private decimal GrandTotal => Subtotal + TaxAmount;
        private int _clientId;
        public ReturnPage()
        {
            this.InitializeComponent();

            _providerService = new ProviderService(DatabaseConfig.ConnectionString);
            _productService = new ProductService(DatabaseConfig.ConnectionString);
            _invoiceService = new InvoiceService(DatabaseConfig.ConnectionString);
            _clientService = new ClientService(DatabaseConfig.ConnectionString);
            ReturnItemsList.ItemsSource = ReturnItems;
            ReturnItems.CollectionChanged += (s, e) =>
            {
                // 1. If new items are added to the list, start listening to their properties
                if (e.NewItems != null)
                {
                    foreach (InvoiceItem item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }

                // 2. If items are removed, stop listening to avoid memory leaks
                if (e.OldItems != null)
                {
                    foreach (InvoiceItem item in e.OldItems)
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                }

                UpdateTotals();
            };

            UpdateTotals();
        }
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If the Quantity changed, recalculate the grand totals
            if (e.PropertyName == nameof(InvoiceItem.Quantity))
            {
                UpdateTotals();
            }
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is InvoiceItem item)
            {
                ReturnItems.Remove(item);
            }
        }
        private async void InvoiceBox_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            int invoice = int.TryParse(InvoiceBox.Text?.Trim(), out var temp) ? temp : 0;
            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = $"Tenga en cuenta que la factura {invoice}" });
            stack.Children.Add(new TextBlock { Text = "Ya fue usada en una devolución" });
            stack.Children.Add(new TextBlock { Text = "De click en CANCELAR para ingresar un nuevo numero de factura" });
            if (invoice == 0)
                return;
            Invoices.Clear();
            if (_invoiceService.InvoiceReturned(invoice))
            {
                var dialog = new ContentDialog
                {
                    Title = "Factura ya usada para devolución",
                    Content = stack,
                    PrimaryButtonText = "Cancelar",
                    CloseButtonText = "Continuar",
                    XamlRoot = this.XamlRoot
                };
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary) 
                {
                    InvoiceBox.Text = "";
                    return;
                }
            }
            _invoiceService.GetByInvoice(invoice).ForEach(i => Invoices.Add(i));
            ItemsList.ItemsSource = Invoices;
            var client = _clientService.GetByInvoice(invoice);

            if (client == null)
            {
                // New client – clear fields but keep DNI
                NameBox.Text = "";
                EmailBox.Text = "";
                PhoneBox.Text = "";
                AddressBox.Text = "";
                DebtBox.Text = "";
                return;
            }

            // Existing client – auto-fill
            _clientId = client.Id;
            DniBox.Text = client.DNI;
            NameBox.Text = client.Name;
            EmailBox.Text = client.Email;
            PhoneBox.Text = client.Phone;
            AddressBox.Text = client.Address;
            DebtBox.Text = client.Debt.ToString("N2");
        }
        private void ReturnItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Get the item from the Button's Tag
            if (sender is Button btn && btn.Tag is InvoiceItem selectedItem)
            {
                var existing = ReturnItems.FirstOrDefault(i => i.ProductId == selectedItem.ProductId);

                if (existing == null)
                {
                    ReturnItems.Add(new InvoiceItem
                    {
                        ProductId = selectedItem.ProductId,
                        ProductName = selectedItem.ProductName,
                        UnitPrice = selectedItem.UnitPrice,
                        MaxQuantity = selectedItem.Quantity, // Set the limit here!
                        Quantity = 1
                    });
                }
                else if (existing.Quantity < existing.MaxQuantity)
                {
                    // Increment if already there, up to the max allowed
                    existing.Quantity++;
                 }
            }
        }
        private void UpdateTotals()
        {
            // These now use your existing class properties (Subtotal, TaxAmount, GrandTotal)
            SubtotalText.Text = Subtotal.ToString("C2");
            //TaxAmountText.Text = TaxAmount.ToString("C2");
            TotalText.Text = GrandTotal.ToString("C2");

            // UI State
            SaveReturnButton.IsEnabled = ReturnItems.Count > 0;
        }
        /*private void TaxBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TaxBox.Text, out var tax))
            {
                _taxRate = tax;
                UpdateTotals();
            }
        }*/
        private async void SaveReturn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ReturnItems.Count == 0)
                return;

            using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1️⃣ CLIENT (Update Data)
                UpdateClient(conn, tx);
                // 2️⃣ INVOICE
                var invoiceId = InsertInvoice(conn, tx);
                // 3️⃣ ITEMS
                foreach (var item in ReturnItems)
                {
                    InsertInvoiceItem(conn, tx, invoiceId, item);
                    UpdateStock(conn, tx, item);
                    InsertMovement(conn, tx, item, invoiceId);
                    //UpdateAveragePrice(conn, tx, item);
                }

                // --- PREPARE DATA FOR DIALOG ---
                var total = ReturnItems.Sum(x => x.UnitPrice * x.Quantity);
                var productSummary = string.Join("\n", ReturnItems.Select(i => $"{i.Quantity}x {i.ProductName}"));
                tx.Commit();

                // --- SHOW CUSTOM DIALOG ---
                await ShowReceiptDialog(invoiceId.ToString(), DniBox.Text, NameBox.Text, productSummary, total);

                // Clear UI...
                InvoiceBox.Text = "";
                DniBox.Text = "";
                DebtBox.Text = "";
                NameBox.Text = "";
                PhoneBox.Text = "";
                EmailBox.Text = "";
                AddressBox.Text = "";
                Invoices.Clear();
                ReturnItems.Clear();
                UpdateTotals();
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
        private async Task ShowReceiptDialog(string invoiceNo, string dni, string name, string products, decimal total)
        {
            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = $"Factura N°: {invoiceNo}", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Cliente: {name} ({dni})" });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray), X2 = 300, StrokeThickness = 1 });
            stack.Children.Add(new TextBlock { Text = "Productos:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            stack.Children.Add(new TextBlock { Text = products, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray), X2 = 300, StrokeThickness = 1 });
            stack.Children.Add(new TextBlock { Text = $"Total: ${total:N2}", HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Saldo a favor: ${GrandTotal:N2}", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red), HorizontalAlignment = HorizontalAlignment.Right });
            

            var dialog = new ContentDialog
            {
                Title = "Devolucion Exitosa",
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
        private void UpdateClient(SqliteConnection conn, SqliteTransaction tx)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Clients 
                SET name = @name,
                    email = @email,
                    phone = @phone,
                    address = @address,
                    debt = debt - @debt
                WHERE id = @client
            """;
            cmd.Parameters.AddWithValue("@name", NameBox.Text);
            cmd.Parameters.AddWithValue("@email", EmailBox.Text);
            cmd.Parameters.AddWithValue("@phone", PhoneBox.Text);
            cmd.Parameters.AddWithValue("@address", AddressBox.Text);
            cmd.Parameters.AddWithValue("@debt", GrandTotal);
            cmd.Parameters.AddWithValue("@client", _clientId);
            cmd.ExecuteNonQuery();
        }
        private int InsertInvoice(SqliteConnection conn, SqliteTransaction tx)
        {
            //int invoice = int.TryParse(InvoiceBox.Text?.Trim(), out var temp) ? temp : 0;
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO ReturnInvoices (client_id, user_id, total, sale_invoice)
                VALUES (@clientId, @userId, @total, @sale_invoice);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@clientId", _clientId);
            cmd.Parameters.AddWithValue("@userId", 1); // TODO: replace with logged user
            cmd.Parameters.AddWithValue("@total", GrandTotal);
            cmd.Parameters.AddWithValue("@sale_invoice", InvoiceBox.Text);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private void InsertInvoiceItem(SqliteConnection conn, SqliteTransaction tx, int invoiceId, InvoiceItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO ReturnInvoiceItems
                (return_id, product_id, quantity)
                VALUES (@invoiceId, @productId, @qty)
            """;

            cmd.Parameters.AddWithValue("@invoiceId", invoiceId);
            cmd.Parameters.AddWithValue("@productId", item.ProductId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);

            cmd.ExecuteNonQuery();
        }
        private void UpdateStock(SqliteConnection conn, SqliteTransaction tx, InvoiceItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Products
                SET stock = stock + @qty
                WHERE id = @id
            """;

            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@id", item.ProductId);

            cmd.ExecuteNonQuery();
        }
        private void InsertMovement(SqliteConnection conn, SqliteTransaction tx, InvoiceItem item, int invoiceId)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO InventoryMovements
                (user_id, product_id, quantity, movement_type, reference)
                VALUES (@userid, @productId, @qty, 'DEVOLUCION', @ref)
            """;
            cmd.Parameters.AddWithValue("@userid", 1); //update later
            cmd.Parameters.AddWithValue("@productId", item.ProductId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@ref", invoiceId);

            cmd.ExecuteNonQuery();
        }
    }
}
