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
                ActiveCategories = ExecuteScalarInt("SELECT COUNT(*) FROM Books WHERE Quantity <= MinStock AND IsActive = 1"),
                InStockValue = 0,
                LowStockAlerts = ExecuteScalarInt("SELECT COUNT(*) FROM Books WHERE CreatedAt >= DATEADD(day, -30, GETDATE()) AND IsActive = 1")
            };
        }

        public async System.Threading.Tasks.Task<CatalogStats> GetStatsAsync()
        {
            return new CatalogStats
            {
                TotalTitles = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Books WHERE IsActive = 1"),
                ActiveCategories = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Books WHERE Quantity <= MinStock AND IsActive = 1"),
                InStockValue = 0,
                LowStockAlerts = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Books WHERE CreatedAt >= DATEADD(day, -30, GETDATE()) AND IsActive = 1")
            };
        }

        public (List<CatalogBookItem> Items, int TotalCount) GetPagedCatalogBooks(int page, int pageSize, string searchTerm = "", int? categoryId = null, string stockStatus = "")
        {
            string whereClause = "WHERE (b.Title LIKE @Search OR b.BookCode LIKE @Search OR b.ISBN LIKE @Search)";

            if (stockStatus == "locked") whereClause += " AND b.IsActive = 0";
            else whereClause += " AND b.IsActive = 1";

            if (categoryId.HasValue)
            {
                whereClause += " AND b.CategoryId = @CategoryId";
            }

            if (stockStatus == "instock") whereClause += " AND b.Quantity > 0";
            else if (stockStatus == "outstock") whereClause += " AND b.Quantity <= 0";

            int totalCount = ExecuteScalarInt($"SELECT COUNT(*) FROM Books b {whereClause}",
                p => {
                    AddParameter(p, "@Search", "%" + searchTerm + "%");
                    if (categoryId.HasValue) AddParameter(p, "@CategoryId", categoryId.Value);
                });

            string query = $@"
                SELECT 
                    b.Id, b.BookCode, b.Title, ISNULL(a.AuthorName, 'Unknown'), ISNULL(c.CategoryName, 'Unknown'), 
                    b.SellingPrice, b.Quantity, b.MinStock, b.IsActive
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
                    bool isActive = reader.GetBoolean(8);
                    
                    string status = "IN STOCK";
                    if (!isActive) status = "LOCKED";
                    else if (qty == 0) status = "OUT OF STOCK";
                    else if (qty <= minStock) status = "LOW STOCK";

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
                if (categoryId.HasValue) AddParameter(p, "@CategoryId", categoryId.Value);
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }

        public async System.Threading.Tasks.Task<(List<CatalogBookItem> Items, int TotalCount)> GetPagedCatalogBooksAsync(int page, int pageSize, string searchTerm = "", int? categoryId = null, string stockStatus = "")
        {
            string whereClause = "WHERE (b.Title LIKE @Search OR b.BookCode LIKE @Search OR b.ISBN LIKE @Search)";

            if (stockStatus == "locked") whereClause += " AND b.IsActive = 0";
            else whereClause += " AND b.IsActive = 1";

            if (categoryId.HasValue)
            {
                whereClause += " AND b.CategoryId = @CategoryId";
            }

            if (stockStatus == "instock") whereClause += " AND b.Quantity > 0";
            else if (stockStatus == "outstock") whereClause += " AND b.Quantity <= 0";

            string countQuery = $"SELECT COUNT(*) FROM Books b {whereClause}";
            int totalCount = await ExecuteScalarAsync<int>(countQuery, new { 
                Search = "%" + searchTerm + "%",
                CategoryId = categoryId
            });

            string query = $@"
                SELECT 
                    b.Id, 
                    b.BookCode AS Isbn13, 
                    b.Title, 
                    ISNULL(a.AuthorName, 'Unknown') AS Author, 
                    ISNULL(c.CategoryName, 'Unknown') AS Category, 
                    b.SellingPrice AS Price,
                    CASE 
                        WHEN b.IsActive = 0 THEN 'LOCKED'
                        WHEN b.Quantity = 0 THEN 'OUT OF STOCK'
                        WHEN b.Quantity <= b.MinStock THEN 'LOW STOCK'
                        ELSE 'IN STOCK'
                    END AS Status
                FROM Books b
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                {whereClause}
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var items = await QueryAsync<CatalogBookItem>(query, new {
                Search = "%" + searchTerm + "%",
                CategoryId = categoryId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });

            return (System.Linq.Enumerable.ToList(items), totalCount);
        }
    }
}
