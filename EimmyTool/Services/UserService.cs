using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

namespace EimmyTool.Services
{
    public class UserService
    {
        private readonly string _cs;
        public UserService(string connectionString) => _cs = connectionString;
        public void Insert(User user)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users (user_name, name, DNI, email, phone, address, password_hash, is_admin) 
                        VALUES (@user, @name, @dni, @email, @phone,@address, @pass, @admin)";

            cmd.Parameters.AddWithValue("@user", user.Username);
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@dni", user.DNI);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@phone", user.Phone);
            cmd.Parameters.AddWithValue("@address", user.Address);
            cmd.Parameters.AddWithValue("@pass", "password"); // Aquí deberías aplicar un Hash en el futuro
            cmd.Parameters.AddWithValue("@admin", user.IsAdmin);

            cmd.ExecuteNonQuery();
        }
        public List<User> GetAll()
        {
            var list = new List<User>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, user_name, name, DNI, email, phone, address, is_admin, is_active FROM Users";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Name = reader.GetString(2),
                    DNI = reader.GetString(3),
                    Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Phone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Address = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    IsAdmin = reader.GetInt32(7),
                    IsActive = reader.GetInt32(8)
                });
            }
            return list;
        }
        public User? GetByDni(string dni)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, user_name, name, DNI, email, phone, address, is_admin, is_active
                FROM Users
                WHERE DNI = @dni
            """;

            cmd.Parameters.AddWithValue("@dni", dni);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Name = reader.GetString(2),
                DNI = reader.GetString(3),
                Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Phone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Address = reader.IsDBNull(6) ? "" :reader.GetString(6),
                IsAdmin = reader.GetInt32(7),
                IsActive = reader.GetInt32(8)
            };

            
        }
    }
}
