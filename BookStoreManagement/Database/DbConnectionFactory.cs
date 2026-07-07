using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookStoreManagement.Database
{
    public class DbConnectionFactory
    {
        private static readonly string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=BookStoreDB;Integrated Security=True";

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
