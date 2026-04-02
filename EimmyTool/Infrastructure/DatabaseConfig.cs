using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Diagnostics;
using System.IO;

namespace EimmyTool.Infrastructure
{
    public static class DatabaseConfig
    {
        public static string DbPath { get; }
        public static string ConnectionString => $"Data Source={DbPath}";
        private const string SchemaSql = @"
        CREATE TABLE IF NOT EXISTS Users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_name TEXT NOT NULL UNIQUE,
            name TEXT NOT NULL,
            DNI TEXT NOT NULL,
            email TEXT,
            phone TEXT,
            address TEXT,
            password_hash TEXT NOT NULL,
            reset_password INTEGER NOT NULL DEFAULT 1,
            is_admin INTEGER NOT NULL DEFAULT 0, -- 0 = normal, 1 = admin
            is_active INTEGER NOT NULL DEFAULT 1,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        );

        CREATE TABLE IF NOT EXISTS Clients (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            DNI TEXT NOT NULL,
            email TEXT,
            phone TEXT,
            address TEXT,
            debt REAL DEFAULT 0,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        );

        CREATE TABLE IF NOT EXISTS Products (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            description TEXT,
            SKU TEXT UNIQUE,
            cost_price REAL NOT NULL,
            sale_price REAL NOT NULL,
            retail_price REAL,
            stock INTEGER NOT NULL DEFAULT 0,
            is_active INTEGER NOT NULL DEFAULT 1,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        );

        CREATE TABLE IF NOT EXISTS Suppliers (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            DNI TEXT NOT NULL,
            email TEXT,
            phone TEXT,
            address TEXT,
            debt REAL DEFAULT 0,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        );

        CREATE TABLE IF NOT EXISTS SalesInvoices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            client_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            paid_full INTEGER NOT NULL DEFAULT 1,
            paid REAL,
            total REAL NOT NULL,
            FOREIGN KEY (client_id) REFERENCES Clients(id),
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS SalesInvoiceItems (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            invoice_id INTEGER NOT NULL,
            product_id INTEGER NOT NULL,
            quantity INTEGER NOT NULL,
            unit_price REAL NOT NULL,
            total REAL NOT NULL,
            FOREIGN KEY (invoice_id) REFERENCES SalesInvoices(id) ON DELETE CASCADE,
            FOREIGN KEY (product_id) REFERENCES Products(id)
        );

        CREATE TABLE IF NOT EXISTS Salaries (
            id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            salary_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            user_id INTEGER,
            hours_worked REAL,
            hour_cost REAL,
            total_paid REAL,
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS PurchaseInvoices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            supplier_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            paid_full INTEGER NOT NULL DEFAULT 1,
            paid REAL,
            total REAL NOT NULL, 
            supplier_invoice TEXT,
            FOREIGN KEY (supplier_id) REFERENCES Suppliers(id),
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS PurchaseInvoiceItems (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            invoice_id INTEGER NOT NULL,
            product_id INTEGER NOT NULL,
            quantity INTEGER NOT NULL,
            unit_cost REAL NOT NULL,
            total REAL NOT NULL,
            FOREIGN KEY (invoice_id) REFERENCES PurchaseInvoices(id) ON DELETE CASCADE,
            FOREIGN KEY (product_id) REFERENCES Products(id)
        );

        CREATE TABLE IF NOT EXISTS PayDebtInvoices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            client_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            paid REAL,
            FOREIGN KEY (client_id) REFERENCES Clients(id),
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS PayCreditInvoices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            supplier_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            paid REAL,
            FOREIGN KEY (supplier_id) REFERENCES Suppliers(id),
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS InventoryMovements (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            product_id INTEGER NOT NULL,
            quantity INTEGER NOT NULL,
            movement_type TEXT NOT NULL, -- IN / OUT
            reference TEXT,             -- Invoice number or description
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (product_id) REFERENCES Products(id),
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS GeneralExpenses (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            invoice_ref TEXT NOT NULL,
            details TEXT NOT NULL,
            total REAL NOT NULL,
            date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            user_id INT,
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS CashFlow (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            type TEXT,
            detail TEXT,
            value REAL,
            reference_table TEXT,
            reference_id INT,
            user_id INT,
            FOREIGN KEY (user_id) REFERENCES Users(id)
        );

        CREATE TABLE IF NOT EXISTS ReturnInvoices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            client_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            sale_invoice TEXT,
            total REAL NOT NULL,
            FOREIGN KEY (sale_invoice) REFERENCES SalesInvoices(id),
            FOREIGN KEY (user_id) REFERENCES Users(id),
            FOREIGN KEY (client_id) REFERENCES Clients(id)
        );

        CREATE TABLE IF NOT EXISTS ReturnInvoiceItems (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            return_id INTEGER NOT NULL,
            product_id INTEGER NOT NULL,
            quantity INTEGER NOT NULL,
            FOREIGN KEY (return_id) REFERENCES ReturnInvoices(id) ON DELETE CASCADE,
            FOREIGN KEY (product_id) REFERENCES Products(id)
        );

        CREATE TABLE IF NOT EXISTS ReturnSupplierInvoices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            supplier_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            buy_invoice TEXT,
            total REAL NOT NULL,
            FOREIGN KEY (buy_invoice) REFERENCES PurchaseInvoices(id),
            FOREIGN KEY (user_id) REFERENCES Users(id),
            FOREIGN KEY (supplier_id) REFERENCES Suppliers(id)
        );

        CREATE TABLE IF NOT EXISTS ReturnSupplierInvoiceItems (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            return_id INTEGER NOT NULL,
            product_id INTEGER NOT NULL,
            quantity INTEGER NOT NULL,
            FOREIGN KEY (return_id) REFERENCES ReturnSupplierInvoices(id) ON DELETE CASCADE,
            FOREIGN KEY (product_id) REFERENCES Products(id)
        );

        CREATE VIEW IF NOT EXISTS View_ClientMovements AS
            SELECT 
                client_id, 
                invoice_date, 
                ri.id,
                SUM(rii.quantity) AS Quantity,
                'Devolución' AS Description, 
                (total * -1) AS Paid, 
                0 as total
            FROM ReturnInvoices ri
            JOIN ReturnInvoiceItems rii ON rii.return_id = ri.id
            GROUP BY rii.return_id
        UNION ALL
            SELECT 
                client_id, 
                invoice_date, 
                si.id,
                SUM(sii.quantity) AS Quantity,
                'Compra' As Description, 
                paid As Paid,
                si.total as total
            FROM SalesInvoices si
            JOIN SalesInvoiceItems sii ON sii.invoice_id = si.id
            GROUP BY sii.invoice_id
        UNION ALL
            SELECT 
                client_id, 
                invoice_date, 
                id,
                0 AS Quantity,
                'Abono deuda' As Description, 
                paid As Paid,
                paid as total
            FROM PayDebtInvoices pdi;

        CREATE VIEW IF NOT EXISTS View_SuppliersMovements AS
            SELECT 
                supplier_id, 
                invoice_date, 
                ri.id,
                SUM(rii.quantity) AS Quantity,
                'Devolución' AS Description, 
                (total * -1) AS Paid,
                0 as total
            FROM ReturnSupplierInvoices ri
            JOIN ReturnSupplierInvoiceItems rii ON rii.return_id = ri.id
            GROUP BY rii.return_id
        UNION ALL
            SELECT 
                supplier_id, 
                invoice_date, 
                pi.id,
                SUM(pii.quantity) AS Quantity,
                'Compra' As Description, 
                paid As Paid,
                pi.total as total
            FROM PurchaseInvoices pi
            JOIN PurchaseInvoiceItems pii ON pii.invoice_id = pi.id
            GROUP BY pii.invoice_id
        UNION ALL
            SELECT 
                supplier_id, 
                invoice_date, 
                id,
                0 AS Quantity,
                'Abono deuda' As Description, 
                paid As Paid,
                paid as total
            FROM PayCreditInvoices;";

        static DatabaseConfig()
        {
#if DEBUG
            // Using a relative path in Debug to avoid Z:\ drive dependency issues
            DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "EimmyTool.db");
            /*DbPath = Path.Combine(
                @"Z:\Eimmy_Teens\EimmyTool\EimmyTool\Data",
                "EimmyTool.db"
            );*/
#else
        DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EimmyTool",
            "EimmyTool.db"
        );
#endif

            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            try
            {
                // 1. Ensure directory exists
                string directory = Path.GetDirectoryName(DbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 2. Create tables if they don't exist
                using (var conn = new SqliteConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = SchemaSql;
                    cmd.ExecuteNonQuery();

                    // 3. Seed the Global Admin
                    EnsureGlobalAdmin(conn);
                }

                Debug.WriteLine($"Database initialized at: {DbPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database Initialization Error: {ex.Message}");
                // In a real app, you might want to log this to a file or show a message
            }
        }
        private static void EnsureGlobalAdmin(SqliteConnection conn)
        {
            conn.Open();
            var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users";
            long count = (long)checkCmd.ExecuteScalar();

            if (count == 0)
            {
                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                INSERT INTO Users (user_name, name, DNI, password_hash, is_admin, is_active, reset_password)
                VALUES (@user, @name, @DNI, @pass, 1, 1, 1)";

                insertCmd.Parameters.AddWithValue("@user", "admin");
                insertCmd.Parameters.AddWithValue("@name", "Global Administrator");
                insertCmd.Parameters.AddWithValue("@DNI", "12345");

                // Set the default password to 'admin123' (Hashed)
                insertCmd.Parameters.AddWithValue("@pass", User.HashPassword("admin123"));

                insertCmd.ExecuteNonQuery();
                Debug.WriteLine("Global Admin created: admin / admin123");
            }
        }
    }
}

