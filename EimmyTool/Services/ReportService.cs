using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace EimmyTool.Services
{
    internal class ReportService
    {
        private readonly string _cs;
        public ReportService(string connectionString)
        {
            _cs = connectionString;
        }
        public (List<SalesReportItem>, decimal Total, int Count, decimal Paid) GetInvoicesByDate(DateTime start, DateTime end)
        {
            var list = new List<SalesReportItem>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT s.id as Id, c.name as Client, u.user_name as User, s.total as Total, s.paid as Paid
                FROM SalesInvoices s
                INNER JOIN Clients c ON s.client_id = c.id
                INNER JOIN Users u ON s.user_id = u.id
                WHERE invoice_date >= @start
                AND invoice_date < @end
            """;
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            using var reader = cmd.ExecuteReader();
            decimal total = 0;
            int count = 0;
            decimal paid = 0;
            while (reader.Read())
            {
                var item = new SalesReportItem
                {
                    Id = reader.GetInt32(0),
                    Client = reader.GetString(1),
                    User = reader.GetString(2),
                    Total = reader.GetDecimal(3),
                    Paid = reader.GetDecimal(4)
                };

                list.Add(item);

                paid += item.Paid;
                total += item.Total;
                count++;
            }
            return (list, total, count, paid);
        }
        public (List<SalesReportItem>, decimal Total, int Count, decimal Paid) GetBuysByDate(DateTime start, DateTime end)
        {
            var list = new List<SalesReportItem>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT ps.id as Id, s.name as Supplier, u.user_name as User, ps.total as Total, ps.paid as Paid
                FROM PurchaseInvoices ps
                INNER JOIN Suppliers s ON ps.supplier_id = s.id
                INNER JOIN Users u ON ps.user_id = u.id
                WHERE invoice_date >= @start
                AND invoice_date < @end
            """;
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            using var reader = cmd.ExecuteReader();
            decimal total = 0;
            int count = 0;
            decimal paid = 0;
            while (reader.Read())
            {
                var item = new SalesReportItem
                {
                    Id = reader.GetInt32(0),
                    Client = reader.GetString(1),
                    User = reader.GetString(2),
                    Total = reader.GetDecimal(3),
                    Paid = reader.GetDecimal(4)
                };

                list.Add(item);

                paid += item.Paid;
                total += item.Total;
                count++;
            }
            return (list, total, count, paid);
        }
        public (List<SalesReportMonthItem>, decimal Total, int count) GetMonthSales(DateTime start, DateTime end)
        {
            var list = new List<SalesReportMonthItem>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                    strftime('%d-%m',invoice_date) as day,
                    count(*) as totalInvoices,
                    SUM(total) as totalSales
                FROM SalesInvoices
                WHERE invoice_date BETWEEN @start AND @end
                GROUP BY day
                ORDER BY invoice_date
            """;
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            using var reader = cmd.ExecuteReader();
            decimal total = 0;
            int count = 0;
            while (reader.Read())
            {
                var item = new SalesReportMonthItem
                {
                    Day = reader.GetString(0),
                    TotalInvoices = reader.GetInt32(1),
                    TotalSales = reader.GetDecimal(2),
                };

                list.Add(item);
                total += item.TotalSales;
                count += item.TotalInvoices;
            }
            return (list, total, count);
        }
        public (List<SalesReportMonthItem>, decimal Total, int count) GetMonthBuys(DateTime start, DateTime end)
        {
            var list = new List<SalesReportMonthItem>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                    strftime('%d-%m',invoice_date) as day,
                    count(*) as totalInvoices,
                    SUM(total) as totalSales
                FROM PurchaseInvoices
                WHERE invoice_date BETWEEN @start AND @end
                GROUP BY day
                ORDER BY invoice_date
            """;
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            using var reader = cmd.ExecuteReader();
            decimal total = 0;
            int count = 0;
            while (reader.Read())
            {
                var item = new SalesReportMonthItem
                {
                    Day = reader.GetString(0),
                    TotalInvoices = reader.GetInt32(1),
                    TotalSales = reader.GetDecimal(2),
                };

                list.Add(item);
                total += item.TotalSales;
                count += item.TotalInvoices;
            }
            return (list, total, count);
        }
        public (List<CashFlowItem>, int debits, decimal totalD, int credits, decimal totalC) GetCashFlowByDate(DateTime start, DateTime end)
        {   
            var list = new List<CashFlowItem>();
            int debits = 0;
            decimal totalD = 0;
            int credits = 0;
            decimal totalC = 0;

            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                    detail,
                    reference_id,
                    CASE WHEN type = 'DEBITO' THEN value ELSE 0 END,
                    CASE WHEN type = 'CREDITO' THEN value ELSE 0 END,
                    u.name
                FROM CashFlow cf
                JOIN Users u ON u.id = cf.user_id
                WHERE date >= @start
                AND date < @end
            """;
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new CashFlowItem
                    {
                        Detail = reader.GetString(0),
                        Reference = reader.GetInt32(1),
                        Debit = reader.GetDecimal(2),
                        Credit = reader.GetDecimal(3),
                        User = reader.GetString(4)
                    });
                }
            }
            using var connTotals = new SqliteConnection(_cs);
            connTotals.Open();
            var cmdTotals = conn.CreateCommand();
            cmdTotals.CommandText = """
                SELECT 
                    COUNT(CASE WHEN type = 'DEBITO' THEN 1 END),
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END),
                    COUNT(CASE WHEN type = 'CREDITO' THEN 1 END),
                    SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END)
                FROM CashFlow
                WHERE date >= @start AND date < @end
            """;
            cmdTotals.Parameters.AddWithValue("@start", start);
            cmdTotals.Parameters.AddWithValue("@end", end);
            using (var reader = cmdTotals.ExecuteReader())
            {
                if (reader.Read())
                {
                    debits = reader.GetInt32(0);
                    totalD = reader.GetDecimal(1);
                    credits = reader.GetInt32(2);
                    totalC = reader.GetDecimal(3);
                }
            }
            return (list, debits, totalD, credits, totalC);
        }
        public (List<CashFlowItem>, int debits, decimal totalD, int credits, decimal totalC) GetMonthlyCashFlow(DateTime start, DateTime end)
        {
            var list = new List<CashFlowItem>();
            int debits = 0;
            decimal totalD = 0;
            int credits = 0;
            decimal totalC = 0;

            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
               SELECT
                    strftime('%d-%m',date,'localtime') as day,
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END) as Entradas,
                    SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END) as Salidas,
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END) 
                        - SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END) as Total
                FROM CashFlow
                WHERE date BETWEEN @start AND @end
                GROUP BY day
                ORDER BY date;
            """;
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new CashFlowItem
                    {
                        Date = reader.GetString(0),
                        Debit = reader.GetDecimal(1),
                        Credit = reader.GetDecimal(2),
                        Total = reader.GetDecimal(3)
                    });
                }
            }
            using var connTotals = new SqliteConnection(_cs);
            connTotals.Open();
            var cmdTotals = conn.CreateCommand();
            cmdTotals.CommandText = """
                SELECT 
                    COUNT(CASE WHEN type = 'DEBITO' THEN 1 END),
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END),
                    COUNT(CASE WHEN type = 'CREDITO' THEN 1 END),
                    SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END)
                FROM CashFlow
                WHERE date >= @start AND date < @end
            """;
            cmdTotals.Parameters.AddWithValue("@start", start);
            cmdTotals.Parameters.AddWithValue("@end", end);
            using (var reader = cmdTotals.ExecuteReader())
            {
                if (reader.Read())
                {
                    debits = reader.GetInt32(0);
                    totalD = reader.GetDecimal(1);
                    credits = reader.GetInt32(2);
                    totalC = reader.GetDecimal(3);
                }
            }
            return (list, debits, totalD, credits, totalC);
        }
        public (List<CashFlowItem>, int debits, decimal totalD, int credits, decimal totalC) GetYearCashFlow(string year)
        {
            var list = new List<CashFlowItem>();
            int debits = 0;
            decimal totalD = 0;
            int credits = 0;
            decimal totalC = 0;

            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
               SELECT
                    strftime('%m',date,'localtime') as month,
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END) as Entradas,
                    SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END) as Salidas,
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END) 
                        - SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END) as Total
                FROM CashFlow
                WHERE strftime('%Y',date,'localtime') = @year
                GROUP BY month
                ORDER BY date;
            """;
            cmd.Parameters.AddWithValue("@year", year);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new CashFlowItem
                    {
                        Date = reader.GetString(0),
                        Debit = reader.GetDecimal(1),
                        Credit = reader.GetDecimal(2),
                        Total = reader.GetDecimal(3)
                    });
                }
            }
            using var connTotals = new SqliteConnection(_cs);
            connTotals.Open();
            var cmdTotals = conn.CreateCommand();
            cmdTotals.CommandText = """
                SELECT 
                    COUNT(CASE WHEN type = 'DEBITO' THEN 1 END),
                    SUM(CASE WHEN type = 'DEBITO' THEN value ELSE 0 END),
                    COUNT(CASE WHEN type = 'CREDITO' THEN 1 END),
                    SUM(CASE WHEN type = 'CREDITO' THEN value ELSE 0 END)
                FROM CashFlow
                WHERE strftime('%Y',date,'localtime') = @year
            """;
            cmdTotals.Parameters.AddWithValue("@year", year);
            using (var reader = cmdTotals.ExecuteReader())
            {
                if (reader.Read())
                {
                    debits = reader.GetInt32(0);
                    totalD = reader.GetDecimal(1);
                    credits = reader.GetInt32(2);
                    totalC = reader.GetDecimal(3);
                }
            }
            return (list, debits, totalD, credits, totalC);
        }
    }
}
