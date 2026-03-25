using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    public class Movement
    {
        public string Date { get; set; }
        public int Invoice {  get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public decimal Paid { get; set; }
        public decimal Total { get; set; }
        
    }
}
