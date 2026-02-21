using EimmyTool.Infrastructure;
using EimmyTool.Models;
using EimmyTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Web.Http;

namespace EimmyTool.Views
{
    public sealed partial class ReportsPage : Page
    {
        private readonly ObservableCollection<SalesReportItem> _sales = new();
        private readonly ObservableCollection<InventoryItem> _stock = new();

        public ReportsPage()
        {
            this.InitializeComponent();

            SalesList.ItemsSource = _sales;
            StockList.ItemsSource = _stock;

            LoadStock();
        }
        private void SalesDateChanged(object sender,DatePickerValueChangedEventArgs args)
        {
            var picker = (DatePicker)sender;
            LoadSales(picker.Date.DateTime);
        }
        private void LoadSales(DateTime localDate)
        {
            _sales.Clear();
            // Local day start/end

            var localStart = localDate.Date;
            var localEnd = localStart.AddDays(1);

            // Convert to UTC
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd);

            using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT s.id as Id, c.name as Client, u.user_name as User, s.total as Total
                FROM SalesInvoices s
                INNER JOIN Clients c ON s.client_id = c.id
                INNER JOIN Users u ON s.user_id = u.id
                WHERE invoice_date >= @start
              AND invoice_date < @end
            """;
            cmd.Parameters.AddWithValue("@start", utcStart);
            cmd.Parameters.AddWithValue("@end", utcEnd);

            using var reader = cmd.ExecuteReader();

            decimal total = 0;
            int count = 0;

            while (reader.Read())
            {
                var item = new SalesReportItem
                {
                    Id = reader.GetInt32(0),
                    Client = reader.GetString(1),
                    User = reader.GetString(2),
                    Total = reader.GetDecimal(3)
                };

                total += item.Total;
                count++;

                _sales.Add(item);
            }

            InvoicesCountText.Text = $"Recibos: {count}";
            SalesTotalText.Text = $"Total: {total:0.00}";
        }
        private void LoadStock()
        {
            _stock.Clear();

            var service = new InventoryService(DatabaseConfig.ConnectionString);

            StockList.ItemsSource = service.GetAll();

            /*using var conn = new SqliteConnection(_cs);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT Name, SKU, Stock, SalePrice
                FROM Products
                WHERE IsActive = 1
            """;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _stock.Add(new StockReportItem
                {
                    Name = reader.GetString(0),
                    SKU = reader.GetString(1),
                    Stock = reader.GetInt32(2),
                    SalePrice = reader.GetDecimal(3)
                });
            }*/
        }
    }
}

