using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SmartTicketing.Data
{
    /// <summary>
    /// Database connection helper for Microsoft SQL Server.
    /// Supports automatic connection testing and fallback.
    /// </summary>
    public static class DatabaseHelper
    {
        // Default LocalDB / Express connection string
        public static string ConnectionString { get; set; } = 
            @"Server=(localdb)\MSSQLLocalDB;Database=SmartTicketingDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;";

        private static bool? _isSqlServerReachable = null;

        /// <summary>
        /// Tests if MSSQL Server is running and reachable.
        /// </summary>
        public static bool CanConnectToSqlServer()
        {
            if (_isSqlServerReachable.HasValue) 
                return _isSqlServerReachable.Value;

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                conn.Open();
                _isSqlServerReachable = true;
                return true;
            }
            catch
            {
                _isSqlServerReachable = false;
                return false;
            }
        }

        public static void ResetConnectionStatus()
        {
            _isSqlServerReachable = null;
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
