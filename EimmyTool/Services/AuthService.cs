using Microsoft.Data.Sqlite;

namespace EimmyTool.Services
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public bool ValidateUser(string username, string passwordHash)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(1)
                FROM Users
                WHERE Username = @username
                  AND PasswordHash = @password
                  AND IsActive = 1
            """;

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", passwordHash);

            var result = (long)cmd.ExecuteScalar();
            return result == 1;
        }
    }
    public static class AppSession
    {
        public static int UserId { get; set; } = 1;
        public static string Username { get; set; } = "admin";
        public static bool IsAdmin { get; set; } = true;
    }
}
