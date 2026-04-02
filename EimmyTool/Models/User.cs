using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace EimmyTool.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string DNI { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public int IsAdmin { get; set; } // 0 o 1
        public int IsActive { get; set; } // 0 o 1
        public string PasswordHash { get; set; }
        public int ResetPassword { get; set; }

        public string Role => IsAdmin == 1 ? "Administrador" : "Usuario";
        public string Status => IsActive == 1 ? "Activo" : "Inactivo";

        // Static property to hold the current logged-in user
        public static User? CurrentUser { get; set; }
        public bool _isAdmin => IsAdmin == 1 ? true : false;
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the string to bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a hex string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}