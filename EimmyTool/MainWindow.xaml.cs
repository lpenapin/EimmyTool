using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EimmyTool
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Default page
            NavView.SelectedItem = NavView.MenuItems[0];
            RootFrame.Navigate(typeof(Views.HomePage));
        }

        private void NavView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Home":
                        RootFrame.Navigate(typeof(Views.HomePage));
                        break;

                    case "Selling":
                        RootFrame.Navigate(typeof(Views.SellingPage));
                        break;

                    case "Buying":
                        RootFrame.Navigate(typeof(Views.BuyPage));
                        break;

                    case "Inventory":
                        RootFrame.Navigate(typeof(Views.InventoryPage));
                        break;

                    case "Reports":
                        RootFrame.Navigate(typeof(Views.ReportsPage));
                        break;

                    case "Contacts":
                        RootFrame.Navigate(typeof(Views.ContactsPage));
                        break;

                    case "Returns":
                        RootFrame.Navigate(typeof(Views.ReturnPage));
                        break;
                }
            }
        }
    }
}
