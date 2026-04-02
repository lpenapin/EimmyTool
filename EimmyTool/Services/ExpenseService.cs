using EimmyTool.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace EimmyTool.Services
{
    class ExpenseService
    {
        private readonly string _cs;
        public ExpenseService(string connectionString)
        {
            _cs = connectionString;
        }
        public int Insert(Expense expense) 
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO GeneralExpenses (invoice_ref, details, total, date, user_id) 
                        VALUES (@invoice_ref, @details, @total, @date, @user_id);
                        SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@invoice_ref", expense.invoice);
            cmd.Parameters.AddWithValue("@details", expense.Details);
            cmd.Parameters.AddWithValue("@total", expense.Total);
            cmd.Parameters.AddWithValue("@date", expense.Date);
            cmd.Parameters.AddWithValue("@user_id", User.CurrentUser.Id);//update later

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        public void InsertCashFlowMovement(int Id, decimal total)
        {
            using var conn = new SqliteConnection(_cs);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO CashFlow
                (type, detail, value, reference_table, reference_id, user_id)
                VALUES ('CREDITO', 'Gasto General', @value, 'GeneralExpenses', @ref_id, @user)
            """;
            cmd.Parameters.AddWithValue("@user", User.CurrentUser.Id); //update later
            cmd.Parameters.AddWithValue("@value", total);
            cmd.Parameters.AddWithValue("@ref_id", Id);

            cmd.ExecuteNonQuery();
        }
    }
}
