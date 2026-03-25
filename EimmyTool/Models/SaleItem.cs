using System;
using System.ComponentModel;

namespace EimmyTool.Models
{
    public class SaleItem : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = "";
        public string Name { get; set; } = "";
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
        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
                OnPropertyChanged(nameof(Total));
            }
        }
        public decimal Total => UnitPrice * Quantity;
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

