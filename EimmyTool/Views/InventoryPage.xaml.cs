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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.NumberFormatting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views
{
    public sealed partial class InventoryPage : Page
    {
        private readonly ObservableCollection<InventoryItem> Items = new();
        private readonly ObservableCollection<InventoryItem> _selectedItem = new();
        private readonly ProductService _productService;
        public InventoryPage()
        {
            this.InitializeComponent();
            _productService = new ProductService(DatabaseConfig.ConnectionString);
            LoadStock();
        }
        private void LoadStock()
        {
            Items.Clear();
            _productService.GetAll().ForEach(x => Items.Add(x));
            StockList.ItemsSource = Items;
        }
        private void SKUbox_LostFocus(object sender, RoutedEventArgs e)
        {
            var sku = SKUbox.Text?.Trim();
            if (string.IsNullOrEmpty(sku))
            {
                SKUText.Text = "";
                LoadStock();
                return;
            }


            var item = _productService.GetBySku(sku);
            if (item != null)
            {
                //Success: Show the name and highlight in list
                SKUText.Text = $"Seleccionado: {item.Name}";
                // Strategy: Filter the list to show ONLY this client
                StockList.ItemsSource = new List<InventoryItem> { item };
            }
            else
            {
                // Fail: Inform the user
                SKUText.Text = "❌ Item no existe";
                StockList.ItemsSource = null;
            }
        }
        private void SKUbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SKUbox_LostFocus(sender, null); // Trigger the search
            }
        }
        private async void AddItem_Click(object sender, RoutedEventArgs e)
        {
            DecimalFormatter decimalFormatter = new DecimalFormatter()
            {
                IsGrouped = true,        // Turns on the thousands separator (e.g., 1,000)
                FractionDigits = 2,      // Enforces 2 decimal places
                IntegerDigits = 1        // Ensures at least one leading zero (e.g., 0.50)
            };
            var txtSKU = new TextBox { Header = "Codigo", Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre", Margin = new Thickness(0, 0, 0, 10) };
            var txtQuantity = new NumberBox { Header = "Cantidad", Margin = new Thickness(0, 0, 0, 10) };
            var txtDescription = new TextBox { Header = "Descripcion", Margin = new Thickness(0, 0, 0, 10) };
            var txtCost = new NumberBox { Header = "Costo", Margin = new Thickness(0, 0, 0, 10), NumberFormatter = decimalFormatter };
            var txtPrice = new NumberBox { Header = "Precio", Margin = new Thickness(0, 0, 0, 10), NumberFormatter = decimalFormatter };
            var txtRetailPrice = new NumberBox { Header = "Precio Mayorista", Margin = new Thickness(0, 0, 0, 10), NumberFormatter = decimalFormatter };
            var cmbReason = new ComboBox
            {
                Header = "Motivo",
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                PlaceholderText = "Seleccione un motivo"
            };
            // Add options to your dropdown
            cmbReason.Items.Add("Nuevo Ingreso");
            cmbReason.Items.Add("Ajuste de Inventario");
            cmbReason.Items.Add("Marca Eimmy");

            var txtError = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };

            var panel = new StackPanel();
            panel.Children.Add(txtSKU);
            panel.Children.Add(txtName);
            panel.Children.Add(txtQuantity);
            panel.Children.Add(txtDescription);
            panel.Children.Add(txtCost);
            panel.Children.Add(txtPrice);
            panel.Children.Add(txtRetailPrice);
            panel.Children.Add(cmbReason);
            panel.Children.Add(txtError);

            var dialog = new ContentDialog
            {
                Title = "Nuevo Producto",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot // Obligatorio en WinUI 3
            };

            dialog.PrimaryButtonClick += (sender, args) =>
            {
                txtError.Visibility = Visibility.Collapsed; // Reset error visibility
                bool isValid = true;

                // Check TextBoxes for empty strings
                if (string.IsNullOrWhiteSpace(txtSKU.Text) ||
                    string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    isValid = false;
                }

                // Check NumberBoxes (WinUI NumberBox value is double.NaN when empty)
                if (double.IsNaN(txtQuantity.Value) ||
                    double.IsNaN(txtCost.Value) ||
                    double.IsNaN(txtPrice.Value) ||
                    double.IsNaN(txtRetailPrice.Value))
                {
                    isValid = false;
                }

                // Check if ComboBox has a selected item
                if (cmbReason.SelectedItem == null)
                {
                    isValid = false;
                }

                // If validation fails, cancel the close action and show the error
                if (!isValid)
                {
                    args.Cancel = true;
                    txtError.Text = "Por favor, complete todos los campos.";
                    txtError.Visibility = Visibility.Visible;
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newItem = new InventoryItem
                {
                    SKU = txtSKU.Text,
                    Name = txtName.Text,
                    Quantity = (int)txtQuantity.Value,
                    Description = txtDescription.Text,
                    Cost = (decimal)txtCost.Value,
                    Price = (decimal)txtPrice.Value,
                    RetailPrice = (decimal)txtRetailPrice.Value,
                };

                var itemId = _productService.Insert(newItem);
                var reason = cmbReason.SelectedItem.ToString();
                _productService.InsertMovement(itemId, newItem.Quantity, reason);
                LoadStock(); // Recarga la lista para mostrar el nuevo producto
            }
        }
        private async void EditItem_Click(object sender, RoutedEventArgs e)
        {
            var item = _selectedItem[0];
            DecimalFormatter decimalFormatter = new DecimalFormatter()
            {
                IsGrouped = true,        // Turns on the thousands separator (e.g., 1,000)
                FractionDigits = 2,      // Enforces 2 decimal places
                IntegerDigits = 1        // Ensures at least one leading zero (e.g., 0.50)
            };
            var txtSKU = new TextBox { Header = "Codigo", Text = item.SKU, Margin = new Thickness(0, 0, 0, 10) };
            var txtName = new TextBox { Header = "Nombre", Text = item.Name, Margin = new Thickness(0, 0, 0, 10) };
            var txtDescription = new TextBox { Header = "Descripcion", Text = item.Description, Margin = new Thickness(0, 0, 0, 10) };
            var txtCost = new NumberBox { Header = "Costo", Value = (double)item.Cost, Margin = new Thickness(0, 0, 0, 10), NumberFormatter = decimalFormatter };
            var txtPrice = new NumberBox { Header = "Precio", Value = (double)item.Price, Margin = new Thickness(0, 0, 0, 10), NumberFormatter = decimalFormatter };
            var txtRetailPrice = new NumberBox { Header = "Precio Mayorista", Value = (double)item.RetailPrice, Margin = new Thickness(0, 0, 0, 10), NumberFormatter = decimalFormatter };

            var txtError = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };

            var panel = new StackPanel();
            panel.Children.Add(txtSKU);
            panel.Children.Add(txtName);
            panel.Children.Add(txtDescription);
            panel.Children.Add(txtCost);
            panel.Children.Add(txtPrice);
            panel.Children.Add(txtRetailPrice);
            panel.Children.Add(txtError);

            var dialog = new ContentDialog
            {
                Title = "Editar Producto",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot // Obligatorio en WinUI 3
            };

            dialog.PrimaryButtonClick += (sender, args) =>
            {
                txtError.Visibility = Visibility.Collapsed; // Reset error visibility
                bool isValid = true;

                // Check TextBoxes for empty strings
                if (string.IsNullOrWhiteSpace(txtSKU.Text) ||
                    string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    isValid = false;
                }

                // Check NumberBoxes (WinUI NumberBox value is double.NaN when empty)
                if (double.IsNaN(txtCost.Value) ||
                    double.IsNaN(txtPrice.Value) ||
                    double.IsNaN(txtRetailPrice.Value))
                {
                    isValid = false;
                }
                // If validation fails, cancel the close action and show the error
                if (!isValid)
                {
                    args.Cancel = true;
                    txtError.Text = "Por favor, complete todos los campos.";
                    txtError.Visibility = Visibility.Visible;
                }
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                item.SKU = txtSKU.Text;
                item.Name = txtName.Text;
                item.Description = txtDescription.Text;
                item.Cost = (decimal)txtCost.Value;
                item.Price = (decimal)txtPrice.Value;
                item.RetailPrice = (decimal)txtRetailPrice.Value;
                try
                {
                    UpdateItem(item);
                }
                catch (Exception ex)
                {
                    // Always wrap DB calls in try-catch to handle locked files or syntax errors
                    Debug.WriteLine($"Database error: {ex.Message}");
                }
                LoadStock();
            }
        }
        private void UpdateItem(InventoryItem item)
        {
            using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    UPDATE Products 
                    SET SKU = @SKU,
                        name = @name,
                        description = @description,
                        cost_price = @cost_price,
                        sale_price = @sale_price,
                        retail_price = @retail_price
                    WHERE id = @item
                """;
                cmd.Parameters.AddWithValue("@SKU", item.SKU);
                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@description", item.Description);
                cmd.Parameters.AddWithValue("@cost_price", item.Cost);
                cmd.Parameters.AddWithValue("@sale_price", item.Price);
                cmd.Parameters.AddWithValue("@retail_price", item.RetailPrice);
                cmd.Parameters.AddWithValue("@item", item.Id);
                cmd.ExecuteNonQuery();
            }
        }
        private void SKUList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StockList.SelectedItem is InventoryItem selectedItem)
            {
                BtnEditItem.IsEnabled = true;
                BtnCantidadItem.IsEnabled = true;
                _selectedItem.Clear();
                _selectedItem.Add(selectedItem);
                /*_selectedItem.Add(new InventoryItem
                {
                    SKU = selectedItem.SKU,
                    Name = selectedItem.Name,
                    Description = selectedItem.Description,
                    Cost = selectedItem.Cost,
                    Price = selectedItem.Price,
                    RetailPrice = selectedItem.RetailPrice
                });*/
            }
            else
            {
                BtnEditItem.IsEnabled = false;
                BtnCantidadItem.IsEnabled = false;
            }
        }
        private async void AjusteCantidadItem_Click(object sender, RoutedEventArgs e)
        {
            var item = _selectedItem[0];
            var itemName = new TextBlock { Text = $"Item: {item.Name} ({item.SKU})" };
            var txtQuantity = new NumberBox { Header = "Cantidad", Value = item.Quantity, Margin = new Thickness(0, 0, 0, 10) };
            var panel = new StackPanel();
            panel.Children.Add(itemName);
            panel.Children.Add(txtQuantity);

            var txtError = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };

            var dialog = new ContentDialog
            {
                Title = "Ajustar Cantidad",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot // Obligatorio en WinUI 3
            };

            dialog.PrimaryButtonClick += (sender, args) =>
            {
                txtError.Visibility = Visibility.Collapsed; // Reset error visibility
                bool isValid = true;

                // Check NumberBoxes (WinUI NumberBox value is double.NaN when empty)
                if (double.IsNaN(txtQuantity.Value))
                {
                    isValid = false;
                }
                // If validation fails, cancel the close action and show the error
                if (!isValid)
                {
                    args.Cancel = true;
                    txtError.Text = "Por favor, complete todos los campos.";
                    txtError.Visibility = Visibility.Visible;
                }
            };
            var originalQuantity = item.Quantity;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                item.Quantity = (int)txtQuantity.Value;
                try
                {
                    var dif = item.Quantity - originalQuantity;
                    _productService.UpdateQuantity(item, item.Quantity);
                    _productService.InsertMovement(item.Id, dif, "Ajuste Inventario");
                }
                catch (Exception ex)
                {
                    // Always wrap DB calls in try-catch to handle locked files or syntax errors
                    Debug.WriteLine($"Database error: {ex.Message}");
                }
                LoadStock();
            }
        }
    }
}
