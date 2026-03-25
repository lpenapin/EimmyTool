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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GeneralExpensesPage : Page
{
    private readonly ExpenseService _expenseService;
    private readonly ObservableCollection<Expense> Expense = new();
    public DateOnly _date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public GeneralExpensesPage()
    {
        InitializeComponent();
        _expenseService = new ExpenseService(DatabaseConfig.ConnectionString);
        cleanBoxs();
    }
    private void cleanBoxs()
    {
        ExpenseDate.Date = DateTimeOffset.Now;
        InvoiceRefBox.Text = "";
        DetailsBox.Text = "";
        PriceBox.Text = "";
    }
    private async void SaveButton_Click(object sender, RoutedEventArgs e) 
    {
        if (ExpenseDate.SelectedDate.HasValue)
        {
            // Extract the DateTime from the DateTimeOffset
            DateTime dt = ExpenseDate.SelectedDate.Value.DateTime;

            // Convert it to DateOnly
            this._date = DateOnly.FromDateTime(dt);

            // Success: Hide error and proceed
            ErrorMessage.Visibility = Visibility.Collapsed;
        }
        // Check if Date is null OR if TextBoxes are empty/whitespace
        bool isInvalid = string.IsNullOrWhiteSpace(InvoiceRefBox.Text) ||
                         string.IsNullOrWhiteSpace(DetailsBox.Text);

        if (isInvalid)
        {
            // Show the error message
            ErrorMessage.Visibility = Visibility.Visible;
        }
        else
        {
            // Hide the error message and save your data
            ErrorMessage.Visibility = Visibility.Collapsed;
            var newExpense = new Expense
            {
                invoice = InvoiceRefBox.Text,
                Details = DetailsBox.Text,
                Total = (decimal)PriceBox.Value,
                Date = _date
            };
            var expenseId = _expenseService.Insert(newExpense);
            _expenseService.InsertCashFlowMovement(expenseId, newExpense.Total);
            cleanBoxs();
        }
    }
    private async void Cancel_Click(object sender, RoutedEventArgs e) 
    {
        cleanBoxs();
    }
}
