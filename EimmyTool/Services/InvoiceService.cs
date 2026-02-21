using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EimmyTool.Services
{
    public class InvoiceService
    {
        private readonly string _cs;
        public InvoiceService(string connectionString) 
        {
            _cs = connectionString;
        }
        public List<InvoiceItem> GetByInvoice(int id)
        { 
            var list = new List<InvoiceItem>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT p.name, s.quantity, s.unit_price, p.id
                FROM SalesInvoiceItems s
                JOIN Products p ON p.id = s.product_id
                WHERE s.invoice_id = @id 
            ";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new InvoiceItem
                {
                    ProductName = reader.GetString(0),
                    MaxQuantity = reader.GetInt32(1),
                    Quantity = reader.GetInt32(1),
                    UnitPrice = reader.GetDecimal(2),
                    ProductId = reader.GetInt32(3)
                });
            }
            return list;
        }
        public bool InvoiceReturned(int id) 
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT count(1)
                FROM ReturnInvoices
                WHERE sale_invoice = @id
            ";
            cmd.Parameters.AddWithValue("@id", id);
            var count = Convert.ToInt32(cmd.ExecuteScalar());

            return count > 0;
        }
    }
}
