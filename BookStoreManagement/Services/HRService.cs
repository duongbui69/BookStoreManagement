using System.Collections.Generic;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class HRService : ServiceBase
    {
        private readonly HRRepository _repo;

        public HRService()
        {
            _repo = new HRRepository();
        }

        public HRStats GetStats()
        {
            return _repo.GetStats();
        }

        public (List<HREmployeeItem> Items, int TotalCount) GetPagedEmployees(int page, int pageSize, string departmentFilter, string statusFilter, string searchTerm)
        {
            return _repo.GetPagedEmployees(page, pageSize, departmentFilter, statusFilter, searchTerm);
        }
    }
}
