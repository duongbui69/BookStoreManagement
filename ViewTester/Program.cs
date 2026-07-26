using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=BookStoreDB;Integrated Security=True;";
        using (var conn = new SqlConnection(connString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT TOP 1 * FROM vw_ReturnReceiptList", conn);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var type = reader.GetFieldType(i);
                        Console.WriteLine($"Column: {name}, Type: {type}");
                    }
                }
            }
        }
    }
}
