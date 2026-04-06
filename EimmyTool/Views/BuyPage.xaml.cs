using EimmyTool.Infrastructure;
using EimmyTool.Models;
using EimmyTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EimmyTool.Views
{
    public sealed partial class BuyPage : Page
    {
        private readonly ProviderService _providerService;
        private readonly ProductService _productService;
        private readonly ObservableCollection<BuyItem> _items = new();
        private int _currentProductId;
        private decimal _taxRate = 0m; // 0%
        private decimal Subtotal => _items.Sum(i => i.Total);
        private decimal TaxAmount => Subtotal * _taxRate / 100m;
        private decimal GrandTotal => Subtotal + TaxAmount;
        private int _currentStock;
        private bool _isNewProduct = false;

        public BuyPage()
        {
            this.InitializeComponent();

            _providerService = new ProviderService(DatabaseConfig.ConnectionString);
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

            var provider = _providerService.GetByDni(dni);

            if (provider == null)
            {
                // New provider – clear fields but keep DNI
                NameBox.Text = "";
                EmailBox.Text = "";
                PhoneBox.Text = "";
                AddressBox.Text = "";
                DebtBox.Text = "";
                return;
            }

            // Existing provider – auto-fill
            NameBox.Text = provider.Name;
            EmailBox.Text = provider.Email;
            PhoneBox.Text = provider.Phone;
            AddressBox.Text = provider.Address;
            DebtBox.Text = provider.Debt.ToString("C2");
        }
        private void SkuBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var sku = SkuBox.Text?.Trim();
            if (string.IsNullOrEmpty(sku))
                return;

            var product = _productService.GetBySku(sku);

            if (product == null)
            {
                ProductNameBox.Text = "";
                ProductNameBox.PlaceholderText = "Ingrese nombre del nuevo producto";
                _currentProductId = 0;
                _currentStock = 0;
                _isNewProduct = true;
                ProductNameBox.Focus(FocusState.Programmatic);
                ProductNameBox.IsReadOnly = false;
                return;
            }

            _isNewProduct = false;
            _currentProductId = product.Id;
            _currentStock = product.Quantity;
            ProductNameBox.Text = product.Name;
            CostBox.Text = product.Cost.ToString("N2");
            PriceBox.Text = product.Price.ToString("N2");
            RetailPriceBox.Text = product.RetailPrice.ToString("N2");
            QuantityBox.Text = "1";
            StockText.Text = $"Stock disponible: {_currentStock}";
        }
        private async void AddItem_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate Inputs first (User must have typed a name and prices)
            if (string.IsNullOrWhiteSpace(ProductNameBox.Text)) return;
            if (string.IsNullOrWhiteSpace(ProductDetailBox.Text)) return;
            if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0) return;
            if (!decimal.TryParse(CostBox.Text, out decimal cost)) return;
            if (!decimal.TryParse(PriceBox.Text, out decimal price)) return;
            if (!decimal.TryParse(RetailPriceBox.Text, out decimal retail_price)) return;

            // 2. Handle New Product Creation
            if (_isNewProduct && _currentProductId == 0)
            {
                try
                {
                    // Insert into DB immediately to get an ID
                    _currentProductId = CreateNewProduct(SkuBox.Text, ProductNameBox.Text, ProductDetailBox.Text, cost, price, retail_price);
                    _isNewProduct = false; // Reset flag
                }
                catch (Exception ex)
                {
                    // Show error if SKU exists or DB fails
                    // ShowErrorDialog(ex.Message); 
                    return;
                }
            }
            // 3. Safety check: If ID is still 0, something went wrong
            if (_currentProductId == 0) return;

            // Quantity already in cart
            var existingQty = _items
                .Where(i => i.ProductId == _currentProductId)
                .Sum(i => i.Quantity);

            _items.Add(new BuyItem
            {
                ProductId = _currentProductId,
                SKU = SkuBox.Text,
                Name = ProductNameBox.Text,
                Quantity = qty,
                UnitCost = cost,
                Price = price,
                RetailPrice = retail_price,
            });

            // Reset product entry
            SkuBox.Text = "";
            ProductNameBox.Text = "";
            QuantityBox.Text = "";
            CostBox.Text = "";
            PriceBox.Text = "";
            RetailPriceBox.Text = "";
            _currentProductId = 0;
            _currentStock = 0;
            _isNewProduct = false;
            StockText.Text = "";
            UpdateTotals();
            SaveBuyButton.IsEnabled = true;
            if (SupplierInvoice.Text != string.Empty) {SupplierInvoice.IsEnabled = false;}
            
        }
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
            if (sender is Button btn && btn.Tag is BuyItem item)
            {
                _items.Remove(item);
                UpdateTotals();
            }
        }
        private async void SavePurcharse_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
                return;
            // --- Validation for Credit ---
            decimal abono = 0;
            if (CreditBuyCheckBox.IsChecked == true)
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
                        Content = "Ingrese la el valor abobado o desmarque la casilla si es de contado",
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
                // 1️⃣ PROVEEDOR (find or create)
                if (string.IsNullOrEmpty(DniBox.Text))
                {
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = "Ingrese la información del proveedor",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                    return;
                }
                var proveedorId = GetOrCreateProveedor(conn, tx);

                // 2️⃣ INVOICE
                var invoiceId = InsertInvoice(conn, tx, proveedorId, abono);

                // 3️⃣ ITEMS
                foreach (var item in _items)
                {
                    InsertInvoiceItem(conn, tx, invoiceId, item);
                    UpdateStock(conn, tx, item);
                    InsertMovement(conn, tx, item, invoiceId);
                    UpdateCost(conn, tx, item);
                }
                // 4 CASH FLOW
                InsertCashFlowMovement(conn, tx, invoiceId, abono);

                // --- PREPARE DATA FOR DIALOG ---
                var total = _items.Sum(x => x.UnitCost * x.Quantity); 
                decimal debt = 0;
                if (CreditBuyCheckBox.IsChecked == true)
                {
                    debt = total - abono;
                    UpdateProviderDebt(conn, tx, proveedorId, debt);
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
                CreditBuyCheckBox.IsChecked = false;
                SupplierInvoice.IsEnabled = true;
                SupplierInvoice.Text = string.Empty;
                _items.Clear();
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
        private async Task ShowReceiptDialog(string invoiceNo, string dni, string name, string products, decimal total, decimal debt)
        {
            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock { Text = $"Factura N°: {invoiceNo}", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Proveedor: {name} ({dni})" });
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
                Title = "Compra Exitosa",
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
        private int GetOrCreateProveedor(SqliteConnection conn, SqliteTransaction tx)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                SELECT id FROM Suppliers WHERE DNI = @dni
            """;
            cmd.Parameters.AddWithValue("@dni", DniBox.Text);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                cmd.Transaction = tx;
                cmd.CommandText = """
                    UPDATE Suppliers 
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
                INSERT INTO Suppliers (name, DNI, email, phone, address)
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
        private int InsertInvoice(SqliteConnection conn, SqliteTransaction tx, int supplierId, decimal abono)
        {
            var PaidFull = 1;
            var Paid = GrandTotal;
            if (CreditBuyCheckBox.IsChecked == true)
            {
                PaidFull = 0;
                Paid = abono;
            }
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO PurchaseInvoices (supplier_id, user_id, total, paid_full, paid, supplier_invoice)
                VALUES (@supplierId, @userId, @total, @paid_full, @paid, @supplierInvoice);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@supplierId", supplierId);
            cmd.Parameters.AddWithValue("@userId", User.CurrentUser.Id); // TODO: replace with logged user
            cmd.Parameters.AddWithValue("@total", GrandTotal);
            cmd.Parameters.AddWithValue("@paid_full", PaidFull);
            cmd.Parameters.AddWithValue("@paid", Paid);
            cmd.Parameters.AddWithValue("@supplierInvoice", SupplierInvoice.Text);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private void InsertInvoiceItem(SqliteConnection conn, SqliteTransaction tx, int invoiceId, BuyItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO PurchaseInvoiceItems
                (invoice_id, product_id, quantity, unit_cost, total)
                VALUES (@invoiceId, @productId, @qty, @price, @total)
            """;

            cmd.Parameters.AddWithValue("@invoiceId", invoiceId);
            cmd.Parameters.AddWithValue("@productId", item.ProductId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@price", item.UnitCost);
            cmd.Parameters.AddWithValue("@total", item.Total);

            cmd.ExecuteNonQuery();
        }
        private void UpdateProviderDebt(SqliteConnection conn, SqliteTransaction tx, int clientId, decimal debt)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Suppliers 
                SET debt = debt + @debt
                WHERE id = @client
            """;

            cmd.Parameters.AddWithValue("@debt", debt);
            cmd.Parameters.AddWithValue("@client", clientId);

            cmd.ExecuteNonQuery();
        }
        private void UpdateStock(SqliteConnection conn, SqliteTransaction tx, BuyItem item)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Products
                SET Stock = Stock + @qty
                WHERE id = @id
            """;

            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@id", item.ProductId);

            cmd.ExecuteNonQuery();
        }
        private void InsertMovement(SqliteConnection conn, SqliteTransaction tx, BuyItem item, int invoiceId)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO InventoryMovements
                (user_id, product_id, quantity, movement_type, reference)
                VALUES (@userid, @productId, @qty, 'COMPRA', @ref)
            """;
            cmd.Parameters.AddWithValue("@userid", User.CurrentUser.Id); //update later
            cmd.Parameters.AddWithValue("@productId", item.ProductId);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@ref", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private void InsertCashFlowMovement(SqliteConnection conn, SqliteTransaction tx, int invoiceId, decimal abono)
        {
            var Paid = GrandTotal;
            if (CreditBuyCheckBox.IsChecked == true)
            {
                Paid = abono;
            }
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO CashFlow
                (type, detail, value, reference_table, reference_id, user_id)
                VALUES ('CREDITO', 'Pago a proveedor', @value, 'PurchaseInvoices', @ref_id, @user)
            """;
            cmd.Parameters.AddWithValue("@user", User.CurrentUser.Id); //update later
            cmd.Parameters.AddWithValue("@value", Paid);
            cmd.Parameters.AddWithValue("@ref_id", invoiceId);

            cmd.ExecuteNonQuery();
        }
        private void UpdateCost(SqliteConnection conn, SqliteTransaction tx, BuyItem item)
        {
            var cmd = conn.CreateCommand();
            cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                UPDATE Products 
                SET cost_price = @cost  
                WHERE id = @id
            """;

            cmd.Parameters.AddWithValue("@cost", item.UnitCost);
            cmd.Parameters.AddWithValue("@id", item.ProductId);

            cmd.ExecuteNonQuery();
        }
        private void CreditBuy_TwoState_Checked(object sender, RoutedEventArgs e)
        {
            // Enable the box when checked
            CreditBox.IsEnabled = true;

            // Optional: Set focus to the box immediately for a better user experience
            CreditBox.Focus(FocusState.Programmatic);
        }
        private void CreditBuy_TwoState_Unchecked(object sender, RoutedEventArgs e)
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
        private void CostBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(CostBox.Text, out decimal amount))
            {
                CostBox.Text = amount.ToString("N2");
            }
        }
        private void PricetBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(PriceBox.Text, out decimal amount))
            {
                PriceBox.Text = amount.ToString("N2");
            }
        }
        private void RetailPriceBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(RetailPriceBox.Text, out decimal amount))
            {
                RetailPriceBox.Text = amount.ToString("N2");
            }
        }
        private int CreateNewProduct(string sku, string name, string description, decimal cost, decimal price, decimal retailPrice)
        {
            using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = """
                INSERT INTO Products (SKU, name, description, cost_price, sale_price, retail_price, stock)
                VALUES (@sku, @name, @description, @cost, @price, @retail, 0);
                SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("@sku", sku);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@cost", cost);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@retail", retailPrice);
            cmd.Parameters.AddWithValue("@description", description);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}


