using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class InventoryStats
    {
        public int TotalItems { get; set; }
        public int OutOfStock { get; set; }
        public decimal StockValue { get; set; }
    }

    public class InventoryDisplayItem
    {
        public int BookId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string PublisherName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
    }

    public class InventoryRepository : RepositoryBase
    {
        public InventoryStats GetStats()
        {
            return new InventoryStats
            {
                TotalItems = ExecuteScalarInt("SELECT ISNULL(SUM(ISNULL(StockNorth, 0) + ISNULL(StockWest, 0) + ISNULL(StockCentral, 0)), 0) FROM Books WHERE IsActive = 1"),
                OutOfStock = ExecuteScalarInt("SELECT COUNT(*) FROM Books WHERE (ISNULL(StockNorth, 0) + ISNULL(StockWest, 0) + ISNULL(StockCentral, 0)) = 0 AND IsActive = 1"),
                StockValue = ExecuteScalarDecimal("SELECT ISNULL(SUM((ISNULL(StockNorth, 0) + ISNULL(StockWest, 0) + ISNULL(StockCentral, 0)) * SellingPrice), 0) FROM Books WHERE IsActive = 1")
            };
        }

        public async System.Threading.Tasks.Task<InventoryStats> GetStatsAsync()
        {
            return new InventoryStats
            {
                TotalItems = await ExecuteScalarAsync<int>("SELECT ISNULL(SUM(ISNULL(StockNorth, 0) + ISNULL(StockWest, 0) + ISNULL(StockCentral, 0)), 0) FROM Books WHERE IsActive = 1"),
                OutOfStock = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Books WHERE (ISNULL(StockNorth, 0) + ISNULL(StockWest, 0) + ISNULL(StockCentral, 0)) = 0 AND IsActive = 1"),
                StockValue = await ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM((ISNULL(StockNorth, 0) + ISNULL(StockWest, 0) + ISNULL(StockCentral, 0)) * SellingPrice), 0) FROM Books WHERE IsActive = 1")
            };
        }

        public (List<InventoryDisplayItem> Items, int TotalCount) GetPagedInventoryItems(int page, int pageSize, string searchTerm = "", string warehouseFilter = "", string categoryFilter = "")
        {
            string cte = @"
                WITH CTE_Inventory AS (
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, N'Kho Tổng (Hà Nội)' AS Warehouse, ISNULL(b.StockNorth, 0) AS CurrentStock, b.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM Books b LEFT JOIN Categories c ON b.CategoryId = c.Id LEFT JOIN Authors a ON b.AuthorId = a.Id LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1
                    UNION ALL
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, N'Kho Chi Nhánh (HCM)' AS Warehouse, ISNULL(b.StockWest, 0) AS CurrentStock, b.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM Books b LEFT JOIN Categories c ON b.CategoryId = c.Id LEFT JOIN Authors a ON b.AuthorId = a.Id LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1
                    UNION ALL
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, N'Kho Miền Trung' AS Warehouse, ISNULL(b.StockCentral, 0) AS CurrentStock, b.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM Books b LEFT JOIN Categories c ON b.CategoryId = c.Id LEFT JOIN Authors a ON b.AuthorId = a.Id LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1
                )
            ";

            string whereClause = "WHERE (Title LIKE @Search OR Sku LIKE @Search)";
            if (!string.IsNullOrEmpty(warehouseFilter)) whereClause += " AND Warehouse = @Warehouse";
            if (!string.IsNullOrEmpty(categoryFilter)) whereClause += " AND CategoryName = @Category";

            int totalCount = ExecuteScalarInt($@"{cte} SELECT COUNT(*) FROM CTE_Inventory {whereClause}",
                p => {
                    AddParameter(p, "@Search", "%" + searchTerm + "%");
                    if (!string.IsNullOrEmpty(warehouseFilter)) AddParameter(p, "@Warehouse", warehouseFilter);
                    if (!string.IsNullOrEmpty(categoryFilter)) AddParameter(p, "@Category", categoryFilter);
                });

            string query = $@"{cte}
                SELECT * FROM CTE_Inventory
                {whereClause}
                ORDER BY BookId DESC, Warehouse
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<InventoryDisplayItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int qty = reader.GetInt32(4);
                    int minStock = reader.GetInt32(5);
                    string status = qty == 0 ? "Out of stock" : (qty <= minStock ? "Low stock" : "In stock");

                    results.Add(new InventoryDisplayItem
                    {
                        BookId = reader.GetInt32(0),
                        Sku = reader.GetString(1),
                        Title = reader.GetString(2),
                        Warehouse = reader.GetString(3),
                        CurrentStock = qty,
                        MinStock = minStock,
                        CategoryName = reader.GetString(6),
                        AuthorName = reader.GetString(7),
                        PublisherName = reader.GetString(8),
                        SellingPrice = reader.GetDecimal(9),
                        Status = status
                    });
                }
                return results;
            }, query, p =>
            {
                AddParameter(p, "@Search", "%" + searchTerm + "%");
                if (!string.IsNullOrEmpty(warehouseFilter)) AddParameter(p, "@Warehouse", warehouseFilter);
                if (!string.IsNullOrEmpty(categoryFilter)) AddParameter(p, "@Category", categoryFilter);
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }

        public async System.Threading.Tasks.Task<(List<InventoryDisplayItem> Items, int TotalCount)> GetPagedInventoryItemsAsync(int page, int pageSize, string searchTerm = "", string warehouseFilter = "", string categoryFilter = "")
        {
            string cte = @"
                WITH CTE_Inventory AS (
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, N'Kho Tổng (Hà Nội)' AS Warehouse, ISNULL(b.StockNorth, 0) AS CurrentStock, b.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM Books b LEFT JOIN Categories c ON b.CategoryId = c.Id LEFT JOIN Authors a ON b.AuthorId = a.Id LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1
                    UNION ALL
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, N'Kho Chi Nhánh (HCM)' AS Warehouse, ISNULL(b.StockWest, 0) AS CurrentStock, b.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM Books b LEFT JOIN Categories c ON b.CategoryId = c.Id LEFT JOIN Authors a ON b.AuthorId = a.Id LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1
                    UNION ALL
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, N'Kho Miền Trung' AS Warehouse, ISNULL(b.StockCentral, 0) AS CurrentStock, b.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM Books b LEFT JOIN Categories c ON b.CategoryId = c.Id LEFT JOIN Authors a ON b.AuthorId = a.Id LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1
                )
            ";

            string whereClause = "WHERE (Title LIKE @Search OR Sku LIKE @Search)";
            if (!string.IsNullOrEmpty(warehouseFilter)) whereClause += " AND Warehouse = @Warehouse";
            if (!string.IsNullOrEmpty(categoryFilter)) whereClause += " AND CategoryName = @Category";

            string countQuery = $@"{cte} SELECT COUNT(*) FROM CTE_Inventory {whereClause}";
            int totalCount = await ExecuteScalarAsync<int>(countQuery, new { 
                Search = "%" + searchTerm + "%",
                Warehouse = warehouseFilter,
                Category = categoryFilter
            });

            string query = $@"{cte}
                SELECT * FROM CTE_Inventory
                {whereClause}
                ORDER BY BookId DESC, Warehouse
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var items = await QueryAsync<InventoryDisplayItem>(query, new {
                Search = "%" + searchTerm + "%",
                Warehouse = warehouseFilter,
                Category = categoryFilter,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });

            var list = System.Linq.Enumerable.ToList(items);
            foreach (var item in list)
            {
                item.Status = item.CurrentStock == 0 ? "Out of stock" : (item.CurrentStock <= item.MinStock ? "Low stock" : "In stock");
            }

            return (list, totalCount);
        }

        public bool UpdateStock(int bookId, string warehouse, int currentStock, int minStock)
        {
            string column = warehouse == "Main Warehouse (Hanoi)" ? "StockNorth" : (warehouse == "Branch Warehouse (HCM)" ? "StockWest" : "StockCentral");
            string sql = $@"
                UPDATE Books
                SET {column} = @CurrentStock, MinStock = @MinStock, UpdatedAt = SYSDATETIME()
                WHERE Id = @BookId
            ";
            return ExecuteNonQuery(sql, p => {
                AddParameter(p, "@BookId", bookId);
                AddParameter(p, "@CurrentStock", currentStock);
                AddParameter(p, "@MinStock", minStock);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateStockAsync(int bookId, string warehouse, int currentStock, int minStock)
        {
            string column = warehouse == "Main Warehouse (Hanoi)" ? "StockNorth" : (warehouse == "Branch Warehouse (HCM)" ? "StockWest" : "StockCentral");
            string sql = $@"
                UPDATE Books
                SET {column} = @CurrentStock, MinStock = @MinStock, UpdatedAt = SYSDATETIME()
                WHERE Id = @BookId
            ";
            return await ExecuteAsync(sql, new { BookId = bookId, CurrentStock = currentStock, MinStock = minStock }) > 0;
        }

        public bool DeleteStock(int bookId, string warehouse)
        {
            string column = warehouse == "Main Warehouse (Hanoi)" ? "StockNorth" : (warehouse == "Branch Warehouse (HCM)" ? "StockWest" : "StockCentral");
            string sql = $@"
                UPDATE Books
                SET {column} = 0, UpdatedAt = SYSDATETIME()
                WHERE Id = @BookId
            ";
            return ExecuteNonQuery(sql, p => AddParameter(p, "@BookId", bookId)) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteStockAsync(int bookId, string warehouse)
        {
            string column = warehouse == "Main Warehouse (Hanoi)" ? "StockNorth" : (warehouse == "Branch Warehouse (HCM)" ? "StockWest" : "StockCentral");
            string sql = $@"
                UPDATE Books
                SET {column} = 0, UpdatedAt = SYSDATETIME()
                WHERE Id = @BookId
            ";
            return await ExecuteAsync(sql, new { BookId = bookId }) > 0;
        }
    }
}
