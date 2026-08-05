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

        public async System.Threading.Tasks.Task<ReportStats> GetFinancialReportsAsync()
        {
            return await _repo.GetFinancialReportsAsync();
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<RevenueRecord>> GetRevenueTableAsync(System.DateTime? fromDate, System.DateTime? toDate, bool groupByDay = false)
        {
            return await _repo.GetRevenueTableAsync(fromDate, toDate, groupByDay);
        }
    }
}
