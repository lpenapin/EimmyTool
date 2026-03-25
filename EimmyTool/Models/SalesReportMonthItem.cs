using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    internal class SalesReportMonthItem
    {
        public string Day { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSales { get; set; }
    }
}
