import re

file_path = 'BookStoreManagement/Repositories/InventoryRepository.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# We will regex replace the methods

content = re.sub(r'public InventoryStats GetStats\(\).*?public \(List<InventoryDisplayItem> Items', 
'''public InventoryStats GetStats()
        {
            return new InventoryStats
            {
                TotalItems = ExecuteScalar<int>("SELECT ISNULL(SUM(Quantity), 0) FROM StoreBookInventories WHERE IsActive = 1"),
                OutOfStock = ExecuteScalar<int>("SELECT COUNT(DISTINCT BookId) FROM StoreBookInventories WHERE Quantity <= 0 AND IsActive = 1"),
                StockValue = ExecuteScalar<decimal>("SELECT ISNULL(SUM(s.Quantity * b.SellingPrice), 0) FROM StoreBookInventories s JOIN Books b ON s.BookId = b.Id WHERE s.IsActive = 1")
            };
        }

        public async System.Threading.Tasks.Task<InventoryStats> GetStatsAsync()
        {
            return new InventoryStats
            {
                TotalItems = await ExecuteScalarAsync<int>("SELECT ISNULL(SUM(Quantity), 0) FROM StoreBookInventories WHERE IsActive = 1"),
                OutOfStock = await ExecuteScalarAsync<int>("SELECT COUNT(DISTINCT BookId) FROM StoreBookInventories WHERE Quantity <= 0 AND IsActive = 1"),
                StockValue = await ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(s.Quantity * b.SellingPrice), 0) FROM StoreBookInventories s JOIN Books b ON s.BookId = b.Id WHERE s.IsActive = 1")
            };
        }

        public (List<InventoryDisplayItem> Items''', content, flags=re.DOTALL)

# Replace CTE
cte_pattern = r'WITH CTE_Inventory AS \([\s\S]*?WHERE b\.IsActive = 1\s*\)'
new_cte = '''WITH CTE_Inventory AS (
                    SELECT b.Id AS BookId, b.BookCode AS Sku, b.Title, st.StoreName AS Warehouse, ISNULL(sbi.Quantity, 0) AS CurrentStock, sbi.MinStock, ISNULL(c.CategoryName, '') AS CategoryName, ISNULL(a.AuthorName, '') AS AuthorName, ISNULL(p.PublisherName, '') AS PublisherName, b.SellingPrice
                    FROM StoreBookInventories sbi
                    JOIN Stores st ON sbi.StoreId = st.Id
                    JOIN Books b ON sbi.BookId = b.Id
                    LEFT JOIN Categories c ON b.CategoryId = c.Id
                    LEFT JOIN Authors a ON b.AuthorId = a.Id
                    LEFT JOIN Publishers p ON b.PublisherId = p.Id
                    WHERE b.IsActive = 1 AND sbi.IsActive = 1
                )'''

content = re.sub(cte_pattern, new_cte, content)

# Replace Update/Delete
update_delete_pattern = r'public bool UpdateStock\([\s\S]*$'
new_update_delete = '''public bool UpdateStock(int bookId, string warehouse, int currentStock, int minStock)
        {
            string sql = @"
                UPDATE sbi
                SET sbi.Quantity = @CurrentStock, sbi.MinStock = @MinStock, sbi.UpdatedAt = SYSDATETIME()
                FROM StoreBookInventories sbi
                JOIN Stores st ON sbi.StoreId = st.Id
                WHERE sbi.BookId = @BookId AND st.StoreName = @Warehouse
            ";
            return ExecuteNonQuery(sql, p => {
                AddParameter(p, "@BookId", bookId);
                AddParameter(p, "@Warehouse", warehouse);
                AddParameter(p, "@CurrentStock", currentStock);
                AddParameter(p, "@MinStock", minStock);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateStockAsync(int bookId, string warehouse, int currentStock, int minStock)
        {
            string sql = @"
                UPDATE sbi
                SET sbi.Quantity = @CurrentStock, sbi.MinStock = @MinStock, sbi.UpdatedAt = SYSDATETIME()
                FROM StoreBookInventories sbi
                JOIN Stores st ON sbi.StoreId = st.Id
                WHERE sbi.BookId = @BookId AND st.StoreName = @Warehouse
            ";
            return await ExecuteAsync(sql, new { BookId = bookId, Warehouse = warehouse, CurrentStock = currentStock, MinStock = minStock }) > 0;
        }

        public bool DeleteStock(int bookId, string warehouse)
        {
            string sql = @"
                UPDATE sbi
                SET sbi.Quantity = 0, sbi.UpdatedAt = SYSDATETIME()
                FROM StoreBookInventories sbi
                JOIN Stores st ON sbi.StoreId = st.Id
                WHERE sbi.BookId = @BookId AND st.StoreName = @Warehouse
            ";
            return ExecuteNonQuery(sql, p => {
                AddParameter(p, "@BookId", bookId);
                AddParameter(p, "@Warehouse", warehouse);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteStockAsync(int bookId, string warehouse)
        {
            string sql = @"
                UPDATE sbi
                SET sbi.Quantity = 0, sbi.UpdatedAt = SYSDATETIME()
                FROM StoreBookInventories sbi
                JOIN Stores st ON sbi.StoreId = st.Id
                WHERE sbi.BookId = @BookId AND st.StoreName = @Warehouse
            ";
            return await ExecuteAsync(sql, new { BookId = bookId, Warehouse = warehouse }) > 0;
        }
    }
}
'''

content = re.sub(update_delete_pattern, new_update_delete, content)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)
