using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string SKU { get; set; } = "";
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal RetailPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }

    }
}
