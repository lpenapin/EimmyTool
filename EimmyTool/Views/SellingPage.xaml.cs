using EimmyTool.Infrastructure;
using EimmyTool.Models;
using EimmyTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Printing;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace EimmyTool.Views
{
    public sealed partial class SellingPage : Page
    {
        private readonly ClientService _clientService;
        private readonly ProductService _productService;
        private readonly ObservableCollection<SaleItem> _items = new();
        private int _currentProductId;
        private decimal _taxRate = 0m; // 0%
        private decimal Subtotal => _items.Sum(i => i.Total);
        private decimal TaxAmount => Subtotal * _taxRate / 100m;
        private decimal GrandTotal => Subtotal - TaxAmount;
        private int _currentStock;
        private decimal _currentPrice;
        private decimal _currentRetailPrice;
        private string _currentProductName;

        public SellingPage()
        {
            this.InitializeComponent();

            _clientService = new ClientService(DatabaseConfig.ConnectionString);
            _productService = new ProductService(DatabaseConfig.ConnectionString);
            ItemsList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => UpdateTotals();
            UpdateTotals();
        }
        private void DniBox_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dni = DniBox.Text?.Trim();

            if (string.IsNullOrEmpty(dni))
                return;

            var client = _clientService.GetByDni(dni);

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
            NameBox.Text = client.Name;
            EmailBox.Text = client.Email;
            PhoneBox.Text = client.Phone;
            AddressBox.Text = client.Address;
            DebtBox.Text = client.Debt.ToString("C2");
        }
        private async void SkuBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var sku = SkuBox.Text?.Trim();
            if (string.IsNullOrEmpty(sku))
                return;

            var product = _productService.GetBySku(sku);

            if (product == null)
            {
                SkuBox.Text = "Product no encontrado";
                _currentProductId = 0;
                _currentStock = 0;
                _currentPrice = 0;
                _currentRetailPrice = 0;
                return;
            }

            _currentProductId = product.Id;
            _currentStock = product.Quantity;
            _currentPrice = product.Price;
            _currentRetailPrice = product.RetailPrice;
            _currentProductName = product.Name;

            if(_currentStock <= 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Stock insuficiente",
                    Content = $"Disponible: {_currentStock}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                return;
            }

            bool isRetail = RetailPriceCheckBox.IsChecked ?? false;
            decimal priceToApply = isRetail ? _currentRetailPrice : _currentPrice;
            var existing = _items.FirstOrDefault(i => i.ProductId == _currentProductId);
            if(existing == null)
            {
                _items.Add(new SaleItem
                {
                    ProductId = _currentProductId,
                    SKU = sku,
                    Name = _currentProductName,
                    MaxQuantity = _currentStock,
                    Quantity = 1,
                    UnitPrice = _currentPrice
                });
            }
            else if (existing.Quantity < existing.MaxQuantity)
            {
                // Increment if already there, up to the max allowed
                existing.Quantity++;
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Límite alcanzado",
                    Content = $"No puedes agregar más de {_currentStock} unidades.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                return;
            }

            SkuBox.Text = string.Empty;
            await Task.Yield();
            SkuBox.Focus(FocusState.Programmatic);
            UpdateTotals();
            SaveSaleButton.IsEnabled = true;
            //UpdatePriceBox();
        }
        /*private async void AddItem_Click(object sender, RoutedEventArgs e)
        {
            decimal price;
            if (_currentProductId == 0)
                return;

            if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
                return;

            if (RetailPriceCheckBox.IsChecked == true)
                {price = _currentRetailPrice;}
            else {price = _currentPrice;}

            // Quantity already in cart
            var existingQty = _items
                .Where(i => i.ProductId == _currentProductId)
                .Sum(i => i.Quantity);

            _items.Add(new SaleItem
            {
                ProductId = _currentProductId,
                SKU = SkuBox.Text,
                Name = ProductNameBox.Text,
                Quantity = qty,
                UnitPrice = price
            });

            // Reset product entry
            SkuBox.Text = "";
            ProductNameBox.Text = "";
            QuantityBox.Text = "";
            PriceBox.Text = "";
            _currentProductId = 0;
            _currentStock = 0;
            StockText.Text = "";
            _currentPrice = 0;
            _currentRetailPrice = 0;
            UpdateTotals();
            SaveSaleButton.IsEnabled = true;
        }*/
        private void UpdateTotals()
        {
            SubtotalText.Text = Subtotal.ToString("C2");
            TaxAmountText.Text = TaxAmount.ToString("C2");
            TotalText.Text = GrandTotal.ToString("C2");
        }
        private void TaxBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TaxBox.Text, out var tax))
            {
                _taxRate = tax;
                UpdateTotals();
            }
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SaleItem item)
            {
                _items.Remove(item);
                UpdateTotals();
            }
        }
        private async void SaveSale_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
                return;
            // --- Validation for Credit ---
            decimal abono = 0;
            if (CreditSaleCheckBox.IsChecked == true)
            {
                if (!decimal.TryParse(CreditBox.Text, out abono))
                {
                    // Show error: Invalid amount
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = "Ingrese un valor valido en abono",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                    return;
                }
                if (abono < 0)
                {
                    // Show error: Invalid amount
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = "Ingrese el valor abobado o desmarque la casilla si es de contado",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                    return;
                }
            }

            using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1️⃣ CLIENT (find or create)
                if (string.IsNullOrEmpty(DniBox.Text))
                {
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = "Ingrese la información del cliente",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                    return;
                }
                var clientId = GetOrCreateClient(conn, tx);

                // 2️⃣ INVOICE
                var invoiceId = InsertInvoice(conn, tx, clientId, abono);

                // 3️⃣ ITEMS
                foreach (var item in _items)
                {
                    InsertInvoiceItem(conn, tx, invoiceId, item);
                    UpdateStock(conn, tx, item);
                    InsertMovement(conn, tx, item, invoiceId);
                    //UpdateAveragePrice(conn, tx, item);
                }
                // 4 CASH FLOW
                InsertCashFlowMovement(conn, tx, invoiceId, abono);


                // --- PREPARE DATA FOR DIALOG ---
                var total = _items.Sum(x => x.UnitPrice * x.Quantity); // Assuming your item has these
                decimal debt = 0;
                if (CreditSaleCheckBox.IsChecked == true)
                {
                    debt = total - abono;
                    UpdateClientDebt(conn, tx, clientId, debt);
                }
                var productSummary = string.Join("\n", _items.Select(i => $"{i.Quantity}x {i.Name}"));
                tx.Commit();

                // --- SHOW CUSTOM DIALOG ---
                await ShowReceiptDialog(invoiceId.ToString(), DniBox.Text, NameBox.Text, productSummary, total, debt);

                // Clear UI...
                DniBox.Text = "";
                NameBox.Text = "";
                EmailBox.Text = "";
                PhoneBox.Text = "";
                AddressBox.Text = "";
                DebtBox.Text = "";
                CreditSaleCheckBox.IsChecked = false;
                //RetailPriceCheckBox.IsChecked = false;
                _items.Clear();
                UpdateTotals();
                SaveSaleButton.IsEnabled = false;
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
        private async Task ShowReceiptDialog(string invoiceNo, string dni, string name, string products, decimal total, decimal debt)
        {
            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = $"Factura N°: {invoiceNo}", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Cliente: {name} ({dni})" });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray), X2 = 300, StrokeThickness = 1 });
            stack.Children.Add(new TextBlock { Text = "Productos:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            stack.Children.Add(new TextBlock { Text = products, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray), X2 = 300, StrokeThickness = 1 });
            stack.Children.Add(new TextBlock { Text = $"Total: ${total:N2}", HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold });

            if (debt > 0)
            {
                stack.Children.Add(new TextBlock { Text = $"Deuda Pendiente: ${debt:N2}", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red), HorizontalAlignment = HorizontalAlignment.Right });
            }

            var dialog = new ContentDialog
            {
                Title = "Venta Exitosa",
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
        private int GetOrCreateClient(SqliteConnection conn, SqliteTransaction tx)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                SELECT Id FROM Clients WHERE DNI = @dni
            """;
            cmd.Parameters.AddWithValue("@dni", DniBox.Text);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                cmd.Transaction = tx;
                cmd.CommandText = """
                    UPDATE Clients 
                    SET name = @name,
                        email = @email,
                        phone = @phone,
                        address = @address
                    WHERE DNI = @client
                """;
                cmd.Parameters.AddWithValue("@name", NameBox.Text);
                cmd.Parameters.AddWithValue("@email", EmailBox.Text);
                cmd.Parameters.AddWithValue("@phone", PhoneBox.Text);
                cmd.Parameters.AddWithValue("@address", AddressBox.Text);
                cmd.Parameters.AddWithValue("@client", DniBox.Text);
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(result);
            }
                
            cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO Clients (name, DNI, email, phone, address)
                VALUES (@name, @dni, @email, @phone, @address);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@name", NameBox.Text);
            cmd.Parameters.AddWithValue("@dni", DniBox.Text);
            cmd.Parameters.AddWithValue("@email", EmailBox.Text);
            cmd.Parameters.AddWithValue("@phone", PhoneBox.Text);
            cmd.Parameters.AddWithValue("@address", AddressBox.Text);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private int InsertInvoice(SqliteConnection conn, SqliteTransaction tx, int clientId, decimal abono)
        {
            var PaidFull = 1;
            var Paid = GrandTotal;
            if (CreditSaleCheckBox.IsChecked == true)
            {
                PaidFull = 0;
                Paid = abono;
            }
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO SalesInvoices (client_id, user_id, total, paid_full, paid)
                VALUES (@clientId, @userId, @total, @paid_full, @paid);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@userId", 1); // TODO: replace with logged user
            cmd.Parameters.AddWithValue("@total", GrandTotal);
            cmd.Parameters.AddWithValue("@paid_full", PaidFull);
            cmd.Parameters.AddWithValue("@paid", Paid);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private void InsertInvoiceItem(SqliteConnection conn, SqliteTransaction tx, int invoiceId, SaleItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO SalesInvoiceItems
                (invoice_id, product_id, quantity, unit_price, total)
                VALUES (@invoiceId, @productId, @qty, @price, @total)
            """;

            cmd.Parameters.AddWithValue("@invoiceId", invoiceId);
            cmd.Parameters.AddWithValue("@productId", item.ProductId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@price", item.UnitPrice);
            cmd.Parameters.AddWithValue("@total", item.Total);

            cmd.ExecuteNonQuery();
        }
        private void UpdateClientDebt(SqliteConnection conn, SqliteTransaction tx, int clientId, decimal debt)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Clients 
                SET debt = debt + @debt
                WHERE id = @client
            """;

            cmd.Parameters.AddWithValue("@debt", debt);
            cmd.Parameters.AddWithValue("@client", clientId);

            cmd.ExecuteNonQuery();
        }
        private void UpdateStock(SqliteConnection conn, SqliteTransaction tx, SaleItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Products
                SET stock = stock - @qty
                WHERE id = @id
            """;

            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@id", item.ProductId);

            cmd.ExecuteNonQuery();
        }
        private void InsertMovement(SqliteConnection conn, SqliteTransaction tx, SaleItem item, int invoiceId)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO InventoryMovements
                (user_id, product_id, quantity, movement_type, reference)
                VALUES (@userid, @productId, @qty, 'VENTA', @ref)
            """;
            cmd.Parameters.AddWithValue("@userid", 1); //update later
            cmd.Parameters.AddWithValue("@productId", item.ProductId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@ref", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private void InsertCashFlowMovement(SqliteConnection conn, SqliteTransaction tx, int invoiceId, decimal abono)
        {
            var Paid = GrandTotal;
            if (CreditSaleCheckBox.IsChecked == true)
            {
                Paid = abono;
            }
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO CashFlow
                (type, detail, value, reference_table, reference_id, user_id)
                VALUES ('DEBITO', 'Venta', @value, 'SalesInvoices', @ref_id, @user)
            """;
            cmd.Parameters.AddWithValue("@user", 1); //update later
            cmd.Parameters.AddWithValue("@value", Paid);
            cmd.Parameters.AddWithValue("@ref_id", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private void PriceBox_TwoState_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePriceBox();
        }
        private void PriceBox_TwoState_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePriceBox();
        }
        private void QuantityBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.DataContext is SaleItem item)
            {
                // Forzamos que la propiedad tenga el valor nuevo que viene en el evento
                item.Quantity = (int)args.NewValue;

                // Ahora sí, recalculamos todo
                UpdateTotals();
            }
        }
        private void UpdatePriceBox()
        {
            bool isRetail = RetailPriceCheckBox.IsChecked ?? false;

            foreach (var item in _items)
            {
                // Necesitamos recuperar el producto original para saber su precio base/mayorista
                // O si ya guardaste ambos precios en SaleItem, úsalos directamente
                var product = _productService.GetBySku(item.SKU);
                if (product != null)
                {
                    item.UnitPrice = isRetail ? product.RetailPrice : product.Price;
                }
            }
            UpdateTotals();
        }
        private void CreditSale_TwoState_Checked(object sender, RoutedEventArgs e) 
        {
            // Enable the box when checked
            CreditBox.IsEnabled = true;

            // Optional: Set focus to the box immediately for a better user experience
            CreditBox.Focus(FocusState.Programmatic);
        }
        private void CreditSale_TwoState_Unchecked(object sender, RoutedEventArgs e) 
        {
            // Disable the box when unchecked
            CreditBox.IsEnabled = false;

            // Optional: Clear the text so no "Abono" value is sent by mistake
            CreditBox.Text = string.Empty;
        }
        private void CreditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(CreditBox.Text, out decimal amount))
            {
                // "N2" provides thousand separators and 2 decimal places
                // The Custom string "00,000.00" forces leading zeros if desired
                CreditBox.Text = amount.ToString("N2");
            }
        }

        /*private void UpdateAveragePrice(SqliteConnection conn, SqliteTransaction tx, SaleItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                SELECT SalePrice FROM Products WHERE Id = @id
            """;
            cmd.Parameters.AddWithValue("@id", item.ProductId);

            var currentPrice = Convert.ToDecimal(cmd.ExecuteScalar());

            if (currentPrice == item.UnitPrice)
                return;

            var avg = (currentPrice + item.UnitPrice) / 2;

            cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Products SET SalePrice = @price WHERE Id = @id
            """;

            cmd.Parameters.AddWithValue("@price", avg);
            cmd.Parameters.AddWithValue("@id", item.ProductId);

            cmd.ExecuteNonQuery();
        }*/

    }
}


