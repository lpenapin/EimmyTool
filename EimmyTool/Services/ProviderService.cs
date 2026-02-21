using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace EimmyTool.Services
{
    public class ProviderService
    {
        private readonly string _cs;

        public ProviderService(string connectionString)
        {
            _cs = connectionString;
        }
        public void Insert(Client client)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Suppliers (name, DNI, email, phone, address) 
                        VALUES (@name, @dni, @email, @phone, @address)";

            cmd.Parameters.AddWithValue("@name", client.Name);
            cmd.Parameters.AddWithValue("@dni", client.DNI);
            cmd.Parameters.AddWithValue("@email", client.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", client.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@address", client.Address ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
        }
        public List<Client> GetAll()
        {
            var list = new List<Client>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            // Cambia "Clients" por "Suppliers" en ProviderService
            cmd.CommandText = "SELECT id, name, DNI, email, phone, address, debt FROM Suppliers";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Client
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    DNI = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Address = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Debt = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6)
                });
            }
            return list;
        }
        public Client? GetByDni(string dni)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, name, DNI, email, phone, address, debt
                FROM Suppliers
                WHERE DNI = @dni
            """;

            cmd.Parameters.AddWithValue("@dni", dni);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new Client
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                DNI = reader.GetString(2),
                Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Address = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Debt = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6)
            };
        }
    }
}
