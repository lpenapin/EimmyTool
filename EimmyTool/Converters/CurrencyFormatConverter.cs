using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Converters
{
    public class CurrencyFormatConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double || value is decimal)
            {
                // This returns the formatted string "$1,234.56"
                return string.Format("{0:C2}", value);
            }
            return value?.ToString() ?? string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
    public class DebtToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Convert to double for the comparison check
            decimal debt = 0;
            if (value is decimal d) debt = d;
            else if (value is double db) debt = (decimal)db;

            if (debt < 0)
            {
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            // Return null to use the default text color (Black/White depending on theme)
            return Microsoft.UI.Xaml.Application.Current.Resources["TextFillColorPrimaryBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
