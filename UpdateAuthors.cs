using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=BookStoreDb;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string sql = @"
                UPDATE Authors SET Nationality = N'Việt Nam' WHERE AuthorName LIKE N'%Thuần' OR AuthorName LIKE N'%Ánh' OR AuthorName LIKE N'%Hoài';
            ";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                int rows = command.ExecuteNonQuery();
                Console.WriteLine($"Updated {rows} rows for Vietnamese authors.");
            }
        }
    }
}
