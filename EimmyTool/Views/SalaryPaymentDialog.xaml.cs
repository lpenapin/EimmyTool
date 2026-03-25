using EimmyTool.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.NumberFormatting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views;
public sealed partial class SalaryPaymentDialog : ContentDialog
{
    // Adjust this to match your actual Employee/Staff model
    public User User { get; set; }

    // WinUI NumberBox binds best to double
    public double HoursWorked { get; set; }
    public double CostPerHour { get; set; }

    // The final calculated total exposed for when the dialog returns
    public decimal TotalSalary { get; private set; }

    public SalaryPaymentDialog(User user)
    {
        this.InitializeComponent();
        this.User = user;
        DecimalFormatter decimalFormatter = new DecimalFormatter()
        {
            IsGrouped = true,        // Turns on the thousands separator (e.g., 1,000)
            FractionDigits = 2,      // Enforces 2 decimal places
            IntegerDigits = 1        // Ensures at least one leading zero (e.g., 0.50)
        };

        // Apply it to the NumberBox
        CostInput.NumberFormatter = decimalFormatter;

        if (App.MainRoot != null)
        {
            this.XamlRoot = App.MainRoot.XamlRoot;
        }
    }
    private void Input_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        // 1. Evitar valores negativos
        if (args.NewValue < 0)
        {
            sender.Value = 0;
            return;
        }

        // 2. Calcular el total cada vez que cambien las horas o el costo
        CalculateTotal();
    }
    private void CalculateTotal()
    {
        // Validate inputs aren't NaN just in case they are cleared
        double hours = double.IsNaN(HoursInput.Value) ? 0 : HoursInput.Value;
        double cost = double.IsNaN(CostInput.Value) ? 0 : CostInput.Value;

        // Convert to decimal for financial accuracy
        TotalSalary = (decimal)(hours * cost);

        // Update the UI
        TotalSalaryText.Text = TotalSalary.ToString("C2");
    }
}
