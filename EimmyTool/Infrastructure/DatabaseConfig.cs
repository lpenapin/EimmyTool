using System;
using System.Diagnostics;
using System.IO;

namespace EimmyTool.Infrastructure
{
    public static class DatabaseConfig
    {
        public static string DbPath { get; }
        public static string ConnectionString => $"Data Source={DbPath}";

        static DatabaseConfig()
        {
#if DEBUG
            // DEV: use fixed path you inspect manually
            DbPath = Path.Combine(
                @"Z:\Eimmy_Teens\EimmyTool\EimmyTool\Data",
                "EimmyTool.db"
            );
#else
            // PROD: per-user safe location
            DbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EimmyTool",
                "EimmyTool.db"
            );
#endif

            Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);

            // First run: seed DB
            if (!File.Exists(DbPath))
            {
                var source = Path.Combine(
                    AppContext.BaseDirectory,
                    "Data",
                    "EimmyTool.db"
                );

                if (!File.Exists(source))
                    throw new FileNotFoundException("Seed database missing", source);

                File.Copy(source, DbPath);

                Debug.WriteLine($"DB PATH: {DatabaseConfig.DbPath}");
            }
        }
    }
}

