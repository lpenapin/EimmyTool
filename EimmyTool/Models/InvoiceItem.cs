using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    public class InvoiceItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ClientId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int MaxQuantity { get; set; }
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                int validatedValue = Math.Clamp(value, 0, MaxQuantity);
                if (_quantity != validatedValue)
                {
                    _quantity = validatedValue;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total));
                }
            }
        }
        public decimal UnitPrice { get; set; }
        public decimal Total => UnitPrice * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
