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

        public DashboardStats GetStats(string filter = "Tháng này")
        {
            return _repo.GetStats(filter);
        }

        public async System.Threading.Tasks.Task<DashboardStats> GetStatsAsync(string filter = "Tháng này")
        {
            return await _repo.GetStatsAsync(filter);
        }
    }
}
