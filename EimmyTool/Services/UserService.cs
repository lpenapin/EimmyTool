using EimmyTool.Infrastructure;
using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using Windows.System;

namespace EimmyTool.Services
{
    public class UserService
    {
        private readonly string _cs;
        public UserService(string connectionString) => _cs = connectionString;
        public void Insert(Models.User user)
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
            cmd.Parameters.AddWithValue("@pass", user.PasswordHash);
            cmd.Parameters.AddWithValue("@admin", user.IsAdmin);

            cmd.ExecuteNonQuery();
        }
        public void UpdateUser(Models.User user)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                    UPDATE Users  
                    SET name = @name,
                        email = @email,
                        phone = @phone,
                        address = @address,
                        DNI = @dni,
                        user_name = @username
                    WHERE id = @user
                """;
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@phone", user.Phone);
            cmd.Parameters.AddWithValue("@address", user.Address);
            cmd.Parameters.AddWithValue("@dni", user.DNI);
            cmd.Parameters.AddWithValue("@user", user.Id);
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.ExecuteNonQuery();
        }
        public List<Models.User> GetAll()
        {
            var list = new List<Models.User>();
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, user_name, name, DNI, email, phone, address, is_admin, is_active FROM Users";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Models.User
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
        public Models.User? GetByDni(string dni)
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

            return new Models.User
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
        public Models.User AuthenticateAndGetUser(string username, string password)
        {
            // Path to your SQLite DB
            using var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                    SELECT id, user_name, name, is_admin,  reset_password
                    FROM Users 
                    WHERE user_name = @user AND password_hash = @pass AND is_active = 1
                    """;

            cmd.Parameters.AddWithValue("@user", username);
            cmd.Parameters.AddWithValue("@pass", password); // Note: Use hashing in production!

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    Models.User.CurrentUser = new Models.User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Name = reader.GetString(2),
                        IsAdmin = reader.GetInt32(3),
                        ResetPassword = reader.GetInt32(4),
                    };
                    return Models.User.CurrentUser;
                }
            }
            return null;
        }
        public bool UpdateUserPassword(int userId, string Hpassword, int reset)
        {
            try
            {
                using var conn = new SqliteConnection(_cs);
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = """
                Update Users 
                SET password_hash = @hashedPass, 
                    reset_password = @reset 
                WHERE id = @id
                """;
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@hashedPass", Hpassword);
                cmd.Parameters.AddWithValue("@reset", reset);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
