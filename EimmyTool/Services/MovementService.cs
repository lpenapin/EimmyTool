using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EimmyTool.Models;
using System.Data;

namespace EimmyTool.Services
{
    class MovementService
    {
        private readonly string _cs;
        public MovementService(string connectionString)
        {
            _cs = connectionString;
        }
        public List<Movement> GetClientHistoryFromDb(int clientId)
        {
            var list = new List<Movement>();

            using var connection = new SqliteConnection(_cs);
            connection.Open();
            var command = connection.CreateCommand();
            // Assuming you have a 'Transactions' table linked by ClientId
            command.CommandText = """
                    SELECT
                        invoice_date, 
                        id, 
                        Quantity, 
                        Description, 
                        Paid, 
                        total
                    FROM View_ClientMovements
                    WHERE client_id = @id
                    ORDER BY invoice_date
                """;
            command.Parameters.AddWithValue("@id", clientId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Movement
                {
                    Date = reader.GetString(0),
                    Invoice = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2),
                    Description = reader.GetString(3),
                    Paid = reader.GetDecimal(4),
                    Total = reader.GetDecimal(5)
                });
            }
            return list;
        }
        public List<Movement> GetSupplierHistoryFromDb(int supplierId)
        {
            var list = new List<Movement>();

            using var connection = new SqliteConnection(_cs);
            connection.Open();
            var command = connection.CreateCommand();
            // Assuming you have a 'Transactions' table linked by ClientId
            command.CommandText = """
                    SELECT
                        invoice_date, 
                        id, 
                        Quantity, 
                        Description, 
                        Paid, 
                        total
                    FROM View_SuppliersMovements
                    WHERE supplier_id = @id
                    ORDER BY invoice_date
                """;
            command.Parameters.AddWithValue("@id", supplierId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Movement
                {
                    Date = reader.GetString(0),
                    Invoice = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2),
                    Description = reader.GetString(3),
                    Paid = reader.GetDecimal(4),
                    Total = reader.GetDecimal(5)
                });
            }
            return list;
        }
        public List<Movement> GetUserHistoryFromDb(int userId)
        {
            var list = new List<Movement>();

            using var connection = new SqliteConnection(_cs);
            connection.Open();
            var command = connection.CreateCommand();
            // Assuming you have a 'Transactions' table linked by ClientId
            command.CommandText = """
                    SELECT
                        salary_date, 
                        id, 
                        hours_worked,
                        total_paid
                    FROM Salaries 
                    WHERE user_id = @id
                    ORDER BY salary_date
                """;
            command.Parameters.AddWithValue("@id", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Movement
                {
                    Date = reader.GetString(0),
                    Invoice = reader.GetInt32(1),
                    Quantity = reader.GetInt32(2),
                    Paid = reader.GetDecimal(3),
                    Total = reader.GetDecimal(3)
                });
            }
            return list;
        }
    }
}
