using System;
using System.Data.SqlClient;

namespace FixAuthorsDb
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Server=(localdb)\mssqllocaldb;Database=BookStoreDB;Trusted_Connection=True;";
            string query = @"
UPDATE Authors SET Nationality = N'Nhật Bản' WHERE Id IN (5, 6, 9, 10, 11, 12, 14, 15, 17, 21, 22, 24, 26, 28, 30, 31, 32, 33, 34, 35, 36);
UPDATE Authors SET Nationality = N'Mỹ' WHERE Id IN (20, 23);
UPDATE Authors SET Nationality = N'Thụy Điển' WHERE Id = 25;
UPDATE Authors SET Nationality = N'Brazil' WHERE Id = 1010;
UPDATE Authors SET Nationality = N'Việt Nam' WHERE Id IN (7, 8, 1012);
UPDATE Authors SET Nationality = N'Khác' WHERE Id IN (1009, 1011);
UPDATE Authors SET AuthorName = N'Nguyễn Nhật Ánh' WHERE Id = 7;
UPDATE Authors SET AuthorName = N'Tô Hoài' WHERE Id = 8;
UPDATE Authors SET AuthorName = N'Tác giả khác' WHERE Id = 1009;
UPDATE Authors SET AuthorName = N'José Mauro de Vasconcelos' WHERE Id = 1010;
UPDATE Authors SET AuthorName = N'Nhiều tác giả' WHERE Id = 1011;
UPDATE Authors SET AuthorName = N'Nguyễn Ngọc Thuần' WHERE Id = 1012;
";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    Console.WriteLine($"Successfully updated {rows} rows with correct Unicode strings.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
