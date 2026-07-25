using System;
using System.Collections.Generic;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class InventoryService : ServiceBase
    {
        private readonly InventoryRepository _repo;

        public InventoryService()
        {
            _repo = new InventoryRepository();
        }

        public InventoryStats GetStats()
        {
            return _repo.GetStats();
        }

        public async System.Threading.Tasks.Task<InventoryStats> GetStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }

        public (List<InventoryDisplayItem> Items, int TotalCount) GetPagedInventoryItems(int page, int pageSize, string searchTerm = "", string warehouseFilter = "", string categoryFilter = "")
        {
            return _repo.GetPagedInventoryItems(page, pageSize, searchTerm, warehouseFilter, categoryFilter);
        }

        public async System.Threading.Tasks.Task<(List<InventoryDisplayItem> Items, int TotalCount)> GetPagedInventoryItemsAsync(int page, int pageSize, string searchTerm = "", string warehouseFilter = "", string categoryFilter = "")
        {
            return await _repo.GetPagedInventoryItemsAsync(page, pageSize, searchTerm, warehouseFilter, categoryFilter);
        }

        public bool UpdateStock(int bookId, string warehouse, int currentStock, int minStock)
        {
            PermissionService.RequireAdmin();
            Require(bookId > 0, "Invalid book ID.");
            Require(currentStock >= 0, "Invalid stock quantity.");
            Require(minStock >= 0, "Invalid min stock.");
            return _repo.UpdateStock(bookId, warehouse, currentStock, minStock);
        }

        public async System.Threading.Tasks.Task<bool> UpdateStockAsync(int bookId, string warehouse, int currentStock, int minStock)
        {
            PermissionService.RequireAdmin();
            Require(bookId > 0, "Invalid book ID.");
            Require(currentStock >= 0, "Invalid stock quantity.");
            Require(minStock >= 0, "Invalid min stock.");
            return await _repo.UpdateStockAsync(bookId, warehouse, currentStock, minStock);
        }

        public bool DeleteStock(int bookId, string warehouse)
        {
            PermissionService.RequireAdmin();
            Require(bookId > 0, "Invalid book ID.");
            return _repo.DeleteStock(bookId, warehouse);
        }

        public async System.Threading.Tasks.Task<bool> DeleteStockAsync(int bookId, string warehouse)
        {
            PermissionService.RequireAdmin();
            Require(bookId > 0, "Invalid book ID.");
            return await _repo.DeleteStockAsync(bookId, warehouse);
        }
    }
}
