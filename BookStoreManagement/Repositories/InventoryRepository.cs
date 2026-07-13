using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class InventoryStats
    {
        public int TotalItems { get; set; }
        public int OutOfStock { get; set; }
        public decimal StockValue { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class InventoryWarehouseItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int NorthHub { get; set; }
        public int WestHub { get; set; }
        public int CentralHub { get; set; }
        public int TotalStock { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class InventoryRepository : RepositoryBase
    {
        public InventoryStats GetStats()
        {
            return new InventoryStats
            {
                TotalItems = ExecuteScalarInt("SELECT ISNULL(SUM(Quantity), 0) FROM Books WHERE IsActive = 1"),
                OutOfStock = ExecuteScalarInt("SELECT COUNT(*) FROM Books WHERE Quantity = 0 AND IsActive = 1"),
                StockValue = ExecuteScalarDecimal("SELECT ISNULL(SUM(Quantity * SellingPrice), 0) FROM Books WHERE IsActive = 1"),
                ReorderPoint = ExecuteScalarInt("SELECT ISNULL(SUM(MinStock), 0) FROM Books WHERE IsActive = 1 AND Quantity <= MinStock")
            };
        }

        public (List<InventoryWarehouseItem> Items, int TotalCount) GetPagedInventoryItems(int page, int pageSize, string searchTerm = "")
        {
            string whereClause = "WHERE b.IsActive = 1 AND (b.Title LIKE @Search OR b.BookCode LIKE @Search)";

            int totalCount = ExecuteScalarInt($"SELECT COUNT(*) FROM Books b {whereClause}",
                p => AddParameter(p, "@Search", "%" + searchTerm + "%"));

            string query = $@"
                SELECT 
                    b.Id, b.Title, b.BookCode, b.SellingPrice, 
                    ISNULL(b.StockNorth, 0), ISNULL(b.StockWest, 0), ISNULL(b.StockCentral, 0), 
                    b.Quantity
                FROM Books b
                {whereClause}
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<InventoryWarehouseItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int qty = reader.GetInt32(7);
                    string status = qty > 10 ? "IN STOCK" : (qty > 0 ? "LOW STOCK" : "OUT OF STOCK");

                    results.Add(new InventoryWarehouseItem
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Sku = "SKU: " + reader.GetString(2),
                        UnitPrice = reader.GetDecimal(3),
                        NorthHub = reader.GetInt32(4),
                        WestHub = reader.GetInt32(5),
                        CentralHub = reader.GetInt32(6),
                        TotalStock = qty,
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
