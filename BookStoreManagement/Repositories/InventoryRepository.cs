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
        public string Title { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int NorthHub { get; set; }
        public int WestHub { get; set; }
        public int CentralHub { get; set; }
        public int TotalStock { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class InventoryRepository
    {
        public InventoryStats GetStats()
        {
            var stats = new InventoryStats();
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Quantity), 0) FROM Books WHERE IsActive = 1", connection))
                stats.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Books WHERE Quantity = 0 AND IsActive = 1", connection))
                stats.OutOfStock = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Quantity * SellingPrice), 0) FROM Books WHERE IsActive = 1", connection))
                stats.StockValue = Convert.ToDecimal(cmd.ExecuteScalar());

            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(MinStock), 0) FROM Books WHERE IsActive = 1 AND Quantity <= MinStock", connection))
                stats.ReorderPoint = Convert.ToInt32(cmd.ExecuteScalar());

            return stats;
        }

        public (List<InventoryWarehouseItem> Items, int TotalCount) GetPagedInventoryItems(int page, int pageSize, string searchTerm = "")
        {
            var list = new List<InventoryWarehouseItem>();
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
                    b.Title, b.BookCode, b.SellingPrice, 
                    ISNULL(b.StockNorth, 0), ISNULL(b.StockWest, 0), ISNULL(b.StockCentral, 0), 
                    b.Quantity
                FROM Books b
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
                int qty = reader.GetInt32(6);
                string status = qty > 10 ? "IN STOCK" : (qty > 0 ? "LOW STOCK" : "OUT OF STOCK");

                list.Add(new InventoryWarehouseItem
                {
                    Title = reader.GetString(0),
                    Sku = "SKU: " + reader.GetString(1),
                    UnitPrice = reader.GetDecimal(2),
                    NorthHub = reader.GetInt32(3),
                    WestHub = reader.GetInt32(4),
                    CentralHub = reader.GetInt32(5),
                    TotalStock = qty,
                    Status = status
                });
            }

            return (list, totalCount);
        }
    }
}
