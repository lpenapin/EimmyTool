using EimmyTool.Infrastructure;
using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace EimmyTool.Services
{
    public class ProductService
    {
        private readonly string _cs;

        public ProductService(string connectionString)
        {
            _cs = connectionString;
        }
        public int Insert(InventoryItem item)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (name, description, SKU, cost_price, sale_price, retail_price, stock) 
                        VALUES (@name, @description, @SKU, @cost_price, @sale_price, @retail_price, @stock);
                        SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@description", item.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SKU", item.SKU);
            cmd.Parameters.AddWithValue("@cost_price", item.Cost);
            cmd.Parameters.AddWithValue("@sale_price", item.Price);
            cmd.Parameters.AddWithValue("@retail_price", item.RetailPrice);
            cmd.Parameters.AddWithValue("@stock", item.Quantity);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        public List<InventoryItem> GetAll()
        {
            var list = new List<InventoryItem>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, name, stock, sale_price, cost_price, retail_price, description, SKU
                FROM Products
                """;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new InventoryItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Quantity = reader.GetInt32(2),
                    Price = reader.GetDecimal(3),
                    Cost = reader.GetDecimal(4),
                    RetailPrice = reader.GetDecimal(5),
                    Description = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    SKU = reader.GetString(7)
                });
            }
            return list;
        }
        public InventoryItem? GetBySku(string sku)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, name, stock, sale_price, cost_price, retail_price, description
                FROM Products
                WHERE SKU = @sku
            """;

            cmd.Parameters.AddWithValue("@sku", sku);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new InventoryItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Cost = reader.GetDecimal(4),
                RetailPrice = reader.GetDecimal(5),
                Description = reader.GetString(6)
            };
        }
        public void InsertMovement(int item,int quantity, string reason)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                    INSERT INTO InventoryMovements
                    (user_id, product_id, quantity, movement_type)
                    VALUES (@userid, @productId, @qty, @reason)
                """;
            cmd.Parameters.AddWithValue("@userid", User.CurrentUser.Id); //update later
            cmd.Parameters.AddWithValue("@productId", item);
            cmd.Parameters.AddWithValue("@qty", quantity);
            cmd.Parameters.AddWithValue("@reason", reason);

            cmd.ExecuteNonQuery();
        }
        public void UpdateQuantity(InventoryItem item, int quantity)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                UPDATE Products 
                SET stock = @quantity
                WHERE id = @item;
            """;
            cmd.Parameters.AddWithValue("@quantity", quantity);
            cmd.Parameters.AddWithValue("@item", item.Id);
            cmd.ExecuteNonQuery();
        }
    }
}

