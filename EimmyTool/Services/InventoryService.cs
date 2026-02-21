using EimmyTool.Infrastructure;
using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace EimmyTool.Services
{
    public class InventoryService
    {
        private readonly string _connectionString;

        public InventoryService(string connectionString)
        {
            _connectionString = $"Data Source={connectionString}";
        }

        public List<InventoryItem> GetAll()
        {
            try
            {
                var list = new List<InventoryItem>();

                using var connection = new SqliteConnection(DatabaseConfig.ConnectionString);
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = """
                        SELECT id, name, description, SKU, cost_price, sale_price, retail_price,  stock, is_active
                        FROM Products
                        ORDER BY name
                    """;

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new InventoryItem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" :reader.GetString(2),
                        SKU = reader.GetString(3),
                        Cost = reader.GetDecimal(4),
                        Price = reader.GetDecimal(5),
                        RetailPrice = reader.GetDecimal(6),
                        Quantity = reader.GetInt32(7),
                        IsActive = reader.GetBoolean(8),
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Inventory DB error: " + ex.Message, ex);
            }
        }
    }
}

