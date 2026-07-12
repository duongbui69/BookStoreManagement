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

        public (List<InventoryWarehouseItem> Items, int TotalCount) GetPagedInventoryItems(int page, int pageSize, string searchTerm = "")
        {
            return _repo.GetPagedInventoryItems(page, pageSize, searchTerm);
        }
    }
}
