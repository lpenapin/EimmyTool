using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    internal class CashFlowItem
    {
        public string Detail { get; set; }
        public int Reference { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string User { get; set; }
        public string Date { get; set; }
        public decimal Total { get; set; }
    }
}
