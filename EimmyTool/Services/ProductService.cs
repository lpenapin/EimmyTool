using Microsoft.Data.Sqlite;

namespace EimmyTool.Services
{
    public class ProductService
    {
        private readonly string _cs;

        public ProductService(string connectionString)
        {
            _cs = connectionString;
        }

        public (int Id, string Name, int Stock, decimal Price, decimal Cost, decimal RetailPrice)? GetBySku(string sku)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, name, stock, sale_price, cost_price, retail_price
                FROM Products
                WHERE SKU = @sku AND is_active = 1
            """;

            cmd.Parameters.AddWithValue("@sku", sku);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return (
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDecimal(3),
                reader.GetDecimal(4),
                reader.GetDecimal(5)
            );
        }
    }
}

