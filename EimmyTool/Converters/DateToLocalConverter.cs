using Microsoft.UI.Xaml.Data;
using System;

namespace EimmyTool.Converters
{
    public class DateToLocalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Verificamos si es string o directamente un DateTime
            if (value is string dateString && DateTime.TryParse(dateString, out DateTime dateFromString))
            {
                return ConvertToLocal(dateFromString);
            }
            else if (value is DateTime dateValue)
            {
                return ConvertToLocal(dateValue);
            }

            return value ?? string.Empty;
        }

        private string ConvertToLocal(DateTime date)
        {
            // Nos aseguramos de que el Kind sea UTC para que ToLocalTime() funcione bien
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return utcDate.ToLocalTime().ToString("dd-MMM-yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
