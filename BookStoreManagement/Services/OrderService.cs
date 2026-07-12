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
    }
}
