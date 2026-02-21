using System.ComponentModel;

namespace EimmyTool.Models
{
    public class BuyItem
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = "";
        public string Name { get; set; } = "";
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
        private decimal _unitCost;
        public decimal UnitCost
        {
            get => _unitCost;
            set
            {
                _unitCost = value;
                OnPropertyChanged(nameof(UnitCost));
                OnPropertyChanged(nameof(Total));
            }
        }
        public decimal Price;
        public decimal RetailPrice;
        public decimal Total => UnitCost * Quantity;
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

