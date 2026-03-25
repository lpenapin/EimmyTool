using EimmyTool.Infrastructure;
using EimmyTool.Models;
using EimmyTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Windows.Globalization.NumberFormatting;
using Windows.Media.Protection.PlayReady;
using Windows.Web.Http;

namespace EimmyTool.Views
{
    public sealed partial class ReportsPage : Page
    {
        private readonly ObservableCollection<SalesReportItem> _sales = new();
        private readonly ObservableCollection<SalesReportItem> _buys = new();
        private readonly ObservableCollection<SalesReportMonthItem> monthItems = new();
        private readonly ObservableCollection<SalesReportMonthItem> buyMonthItems = new();
        private readonly ObservableCollection<CashFlowItem> _cashMovement = new();
        private readonly ObservableCollection<CashFlowItem> cashFlows = new();
        private readonly ObservableCollection<CashFlowItem> YearCashFlows = new();
        public ObservableCollection<string> YearList { get; set; } = new ObservableCollection<string>();
        private readonly ReportService _saleReportService;

        public ReportsPage()
        {
            this.InitializeComponent();
            LoadYears();

            SalesListDaily.ItemsSource = _sales;
            SalesListMonth.ItemsSource = monthItems;
            SalesListDaily.ItemsSource = _buys;
            CashFlowListDaily.ItemsSource = _cashMovement;
            CashFlowListMonth.ItemsSource = cashFlows;
            CashFlowListYear.ItemsSource = YearCashFlows;
            _saleReportService = new ReportService(DatabaseConfig.ConnectionString);
        }
        private void SalesDateChangedDay(object sender, DatePickerValueChangedEventArgs args)
        {
            var picker = (DatePicker)sender;
            LoadSalesDay(picker.Date.DateTime);
        }
        private void LoadSalesDay(DateTime localDate)
        {
            _cashMovement.Clear();
            // Local day start/end

            var localStart = localDate.Date;
            var localEnd = localStart.AddDays(1);

            // Convert to UTC
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd);

            var (items, total, count, paid) = _saleReportService.GetInvoicesByDate(utcStart, utcEnd);
            items.ForEach(i => _sales.Add(i));
            SalesListDaily.ItemsSource = _sales;

            InvoicesCountText.Text = $"Ventas: {count}";
            PaidTotalText.Text = $"Ingreso caja: {paid:C2}";
            SalesTotalText.Text = $"Total ventas: {total:C2}";
        }
        private void SearchInvoices_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs arg)
        {

            var localStart = SalesDatePickerFrom.SelectedDate;
            var localEnd = SalesDatePickerTo.SelectedDate;

            if (localStart == null || localEnd == null)
            {
                // Optional: Show a message to the user that dates are required
                return;
            }

            // Convert to UTC
            var utcStart = localStart.Value.UtcDateTime;
            var utcEnd = localEnd.Value.UtcDateTime;

            monthItems.Clear();
            var (items, total, count) = _saleReportService.GetMonthSales(utcStart, utcEnd);
            items.ForEach(i =>  monthItems.Add(i));
            SalesListMonth.ItemsSource = monthItems;

            InvoicesMonthCountText.Text = $"Ventas: {count}";
            SalesTotalMonthText.Text = $"Total: {total:C2}";

        }
        private void BuysDateChangedDay(object sender, DatePickerValueChangedEventArgs args)
        {
            var picker = (DatePicker)sender;
            LoadBuysDay(picker.Date.DateTime);
        }
        private void LoadBuysDay(DateTime localDate)
        {
            _buys.Clear();
            // Local day start/end

            var localStart = localDate.Date;
            var localEnd = localStart.AddDays(1);

            // Convert to UTC
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd);

            var (items, total, count, paid) = _saleReportService.GetBuysByDate(utcStart, utcEnd);
            items.ForEach(i => _buys.Add(i));
            BuysListDaily.ItemsSource = _buys;

            InvoicesBuyCountText.Text = $"Compras: {count}";
            PaidTotalBuyText.Text = $"Salida caja: {paid:C2}";
            BuysTotalText.Text = $"Total Compras: {total:C2}";
        }
        private void SearchInvoicesBuy_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs arg)
        {

            var localStart = BuysDatePickerFrom.SelectedDate;
            var localEnd = BuysDatePickerTo.SelectedDate;

            if (localStart == null || localEnd == null)
            {
                // Optional: Show a message to the user that dates are required
                return;
            }

            // Convert to UTC
            var utcStart = localStart.Value.UtcDateTime;
            var utcEnd = localEnd.Value.UtcDateTime;

            buyMonthItems.Clear();
            var (items, total, count) = _saleReportService.GetMonthBuys(utcStart, utcEnd);
            items.ForEach(i => buyMonthItems.Add(i));
            BuysListMonth.ItemsSource = buyMonthItems;

            InvoicesBuyMonthCountText.Text = $"Compras: {count}";
            BuysTotalMonthText.Text = $"Total: {total:C2}";

        }
        private void CashFlowDateChangedDay(object sender, DatePickerValueChangedEventArgs args)
        {
            var picker = (DatePicker)sender;
            LoadCashFlowDay(picker.Date.DateTime);
        }
        private void LoadCashFlowDay(DateTime localDate)
        {
            _cashMovement.Clear();
            // Local day start/end

            var localStart = localDate.Date;
            var localEnd = localStart.AddDays(1);

            // Convert to UTC
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd);

            var (items, countD, paidI, countC, paidS) = _saleReportService.GetCashFlowByDate(utcStart, utcEnd);
            items.ForEach(i => _cashMovement.Add(i));
            SalesListDaily.ItemsSource = _cashMovement;

            DebitsCountText.Text = $"Entradas: {countD}";
            TotalDebitsText.Text = $"Ingreso caja: {paidI:C2}";
            CreditsText.Text = $"Salidas: {countC}";
            TotalCreditsText.Text = $"Salidas caja: {paidS:C2}";
        }
        private void SearchCashFlow_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs arg)
        {

            var localStart = CashFlowDatePickerFrom.SelectedDate;
            var localEnd = CashFlowDatePickerTo.SelectedDate;

            if (localStart == null || localEnd == null)
            {
                // Optional: Show a message to the user that dates are required
                return;
            }

            // Convert to UTC
            var utcStart = localStart.Value.UtcDateTime;
            var utcEnd = localEnd.Value.UtcDateTime;

            cashFlows.Clear();
            var (items, countD, paidI, countC, paidS) = _saleReportService.GetMonthlyCashFlow(utcStart, utcEnd);
            items.ForEach(i => cashFlows.Add(i));
            CashFlowListMonth.ItemsSource = cashFlows;

            DebitsMonthCountText.Text = $"Entradas: {countD}";
            TotalDebitsMonthText.Text = $"Ingreso caja: {paidI:C2}";
            CreditsMonthText.Text = $"Salidas: {countC}";
            TotalCreditsMonthText.Text = $"Salidas caja: {paidS:C2}";

        }
        private void ComboYears_SelectionChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs arg)
        {

            var year = ComboYears.SelectedItem.ToString();

            if (year == null)
            {
                // Optional: Show a message to the user that dates are required
                return;
            }

            YearCashFlows.Clear();
            var (items, countD, paidI, countC, paidS) = _saleReportService.GetYearCashFlow(year);
            items.ForEach(i => YearCashFlows.Add(i));
            CashFlowListYear.ItemsSource = YearCashFlows;

            DebitsYearCountText.Text = $"Entradas: {countD}";
            TotalDebitsYearText.Text = $"Ingreso caja: {paidI:C2}";
            CreditsYearText.Text = $"Salidas: {countC}";
            TotalCreditsYearText.Text = $"Salidas caja: {paidS:C2}";

        }
        private void LoadYears()
        {
            using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                // Consultamos solo la parte del año de la columna fecha
                var query = """
                    SELECT DISTINCT strftime('%Y', date) 
                    FROM CashFlow 
                    WHERE date IS NOT NULL 
                    ORDER BY date DESC
                    """;
                var command = new SqliteCommand(query, conn);

                using (var reader = command.ExecuteReader())
                {
                    YearList.Clear();
                    while (reader.Read())
                    {
                        // Agregamos el año como string a la colección
                        YearList.Add(reader.GetString(0));
                    }
                }
            }
        }
    }
}

