using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class ReportService : ServiceBase
    {
        private readonly ReportRepository _repo;

        public ReportService()
        {
            _repo = new ReportRepository();
        }

        public ReportStats GetFinancialReports()
        {
            return _repo.GetFinancialReports();
        }
    }
}
