using System.Collections.Generic;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class OrderService : ServiceBase
    {
        private readonly OrderRepository _repo;

        public OrderService()
        {
            _repo = new OrderRepository();
        }

        public OrderStats GetStats()
        {
            return _repo.GetStats();
        }

        public (List<OrderItem> Items, int TotalCount) GetPagedOrders(int page, int pageSize, string statusFilter, string searchTerm)
        {
            return _repo.GetPagedOrders(page, pageSize, statusFilter, searchTerm);
        }

        public async System.Threading.Tasks.Task<OrderStats> GetStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }

        public async System.Threading.Tasks.Task<(List<OrderItem> Items, int TotalCount)> GetPagedOrdersAsync(int page, int pageSize, string statusFilter, string searchTerm)
        {
            return await _repo.GetPagedOrdersAsync(page, pageSize, statusFilter, searchTerm);
        }
    }
}
