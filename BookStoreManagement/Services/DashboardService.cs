using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class DashboardService : ServiceBase
    {
        private readonly DashboardRepository _repo;

        public DashboardService()
        {
            _repo = new DashboardRepository();
        }

        public DashboardStats GetStats()
        {
            return _repo.GetStats();
        }

        public async System.Threading.Tasks.Task<DashboardStats> GetStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }
    }
}
