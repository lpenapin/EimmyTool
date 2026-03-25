using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Models
{
    class Expense
    {
        public int Id { get; set; }
        public string invoice { get; set; } = "";
        public string Details { get; set; } = "";
        public decimal Total { get; set; }
        public DateOnly Date {  get; set; }
        public int UserId { get; set; }
    }
}
