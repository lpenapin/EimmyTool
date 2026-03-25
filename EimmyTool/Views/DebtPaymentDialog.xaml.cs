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
using Windows.Media.Protection.PlayReady;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DebtPaymentDialog : ContentDialog
{
    public Client Client { get; set; }
    public decimal PaymentAmount { get; set; }
    public double PaymentAmountDouble
    {
        get => (double)PaymentAmount;
        set => PaymentAmount = (decimal)value;
    }
    private void PaymentInput_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        // 1. Evitar valores negativos
        if (args.NewValue < 0)
        {
            sender.Value = 0;
            return;
        }
        // 2. Evitar que pague más de la deuda actual
        if ((decimal)args.NewValue > Client.Debt)
        {
            // Forzamos el valor máximo permitido (la deuda total)
            sender.Value = (double)Client.Debt;
            CurrentDebtText.Text = sender.Value.ToString();
            // Opcional: Podrías mostrar un mensaje visual aquí
        }
    }
    private void PayFull_Click(object sender, RoutedEventArgs e)
    {
        PaymentAmountDouble = (double)Client.Debt;
        PaymentInput.Value = PaymentAmountDouble;
    }
    public DebtPaymentDialog(Client client)
    {
        this.InitializeComponent();
        this.Client = client;
        CurrentDebtText.Text = client.Debt.ToString("C2");
        if (App.MainRoot != null)
        {
            this.XamlRoot = App.MainRoot.XamlRoot;
        }
    }
}
