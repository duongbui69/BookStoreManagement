using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class CatalogStats
    {
        public int TotalTitles { get; set; }
        public int ActiveCategories { get; set; }
        public decimal InStockValue { get; set; }
        public int LowStockAlerts { get; set; }
    }

    public class CatalogBookItem
    {
        public string Isbn13 { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CatalogRepository
    {
        public CatalogStats GetStats()
        {
            var stats = new CatalogStats();
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Books WHERE IsActive = 1", connection))
                stats.TotalTitles = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Categories WHERE IsActive = 1", connection))
                stats.ActiveCategories = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Quantity * SellingPrice), 0) FROM Books WHERE IsActive = 1", connection))
                stats.InStockValue = Convert.ToDecimal(cmd.ExecuteScalar());

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Books WHERE Quantity <= MinStock AND IsActive = 1", connection))
                stats.LowStockAlerts = (int)cmd.ExecuteScalar();

            return stats;
        }

        public (List<CatalogBookItem> Items, int TotalCount) GetPagedCatalogBooks(int page, int pageSize, string searchTerm = "")
        {
            var list = new List<CatalogBookItem>();
            int totalCount = 0;
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            string whereClause = "WHERE b.IsActive = 1 AND (b.Title LIKE @Search OR b.BookCode LIKE @Search)";

            using (var countCmd = new SqlCommand($"SELECT COUNT(*) FROM Books b {whereClause}", connection))
            {
                countCmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                totalCount = (int)countCmd.ExecuteScalar();
            }

            string query = $@"
                SELECT 
                    b.BookCode, b.Title, ISNULL(a.AuthorName, 'Unknown'), ISNULL(c.CategoryName, 'Unknown'), 
                    b.SellingPrice, b.Quantity, b.MinStock
                FROM Books b
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                {whereClause}
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int qty = reader.GetInt32(5);
                int minStock = reader.GetInt32(6);
                string status = "IN STOCK";
                if (qty == 0) status = "OUT OF STOCK";
                else if (qty <= minStock) status = "LOW STOCK";

                list.Add(new CatalogBookItem
                {
                    Isbn13 = reader.GetString(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    Category = reader.GetString(3),
                    Price = reader.GetDecimal(4),
                    Status = status
                });
            }

            return (list, totalCount);
        }
    }
}
