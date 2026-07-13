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
        public int Id { get; set; }
        public string Isbn13 { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CatalogRepository : RepositoryBase
    {
        public CatalogStats GetStats()
        {
            return new CatalogStats
            {
                TotalTitles = ExecuteScalarInt("SELECT COUNT(*) FROM Books WHERE IsActive = 1"),
                ActiveCategories = ExecuteScalarInt("SELECT COUNT(*) FROM Categories WHERE IsActive = 1"),
                InStockValue = ExecuteScalarDecimal("SELECT ISNULL(SUM(Quantity * SellingPrice), 0) FROM Books WHERE IsActive = 1"),
                LowStockAlerts = ExecuteScalarInt("SELECT COUNT(*) FROM Books WHERE Quantity <= MinStock AND IsActive = 1")
            };
        }

        public (List<CatalogBookItem> Items, int TotalCount) GetPagedCatalogBooks(int page, int pageSize, string searchTerm = "")
        {
            string whereClause = "WHERE b.IsActive = 1 AND (b.Title LIKE @Search OR b.BookCode LIKE @Search)";

            int totalCount = ExecuteScalarInt($"SELECT COUNT(*) FROM Books b {whereClause}",
                p => AddParameter(p, "@Search", "%" + searchTerm + "%"));

            string query = $@"
                SELECT 
                    b.Id, b.BookCode, b.Title, ISNULL(a.AuthorName, 'Unknown'), ISNULL(c.CategoryName, 'Unknown'), 
                    b.SellingPrice, b.Quantity, b.MinStock
                FROM Books b
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                {whereClause}
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<CatalogBookItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int qty = reader.GetInt32(6);
                    int minStock = reader.GetInt32(7);
                    string status = qty == 0 ? "OUT OF STOCK" : (qty <= minStock ? "LOW STOCK" : "IN STOCK");

                    results.Add(new CatalogBookItem
                    {
                        Id = reader.GetInt32(0),
                        Isbn13 = reader.GetString(1),
                        Title = reader.GetString(2),
                        Author = reader.GetString(3),
                        Category = reader.GetString(4),
                        Price = reader.GetDecimal(5),
                        Status = status
                    });
                }
                return results;
            }, query, p =>
            {
                AddParameter(p, "@Search", "%" + searchTerm + "%");
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }
    }
}
