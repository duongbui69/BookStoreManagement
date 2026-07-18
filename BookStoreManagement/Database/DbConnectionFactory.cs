using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BookStoreManagement.Database
{
    public class DbConnectionFactory
    {
        private static readonly string connectionString;

        static DbConnectionFactory()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();
            connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=BookStoreDB;Integrated Security=True";
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
