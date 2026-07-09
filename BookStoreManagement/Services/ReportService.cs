using System;
using System.Collections.Generic;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class ReportService : ServiceBase
    {
        private readonly ReportRepository _reportRepository;

        public ReportService()
        {
            _reportRepository = new ReportRepository();
        }

        public decimal GetTodayRevenue() => _reportRepository.GetTodayRevenue();

        public decimal GetMonthRevenue() => _reportRepository.GetMonthRevenue();

        public decimal GetTotalRevenue() => _reportRepository.GetTotalRevenue();

        public decimal GetRevenueByDateRange(DateTime fromDate, DateTime toDate)
        {
            ValidateDateRange(fromDate, toDate);
            return _reportRepository.GetRevenueByDateRange(fromDate.Date, toDate.Date);
        }

        public int GetTotalOrders() => _reportRepository.GetTotalOrders();

        public int GetCompletedOrders() => _reportRepository.GetCompletedOrders();

        public int GetCancelledOrders() => _reportRepository.GetCancelledOrders();

        public int GetTotalBooks() => _reportRepository.GetTotalBooks();

        public int GetTotalUsers() => _reportRepository.GetTotalUsers();

        public int GetTotalCustomers() => _reportRepository.GetTotalCustomers();

        public List<DailyRevenueViewModel> GetDailyRevenue() => _reportRepository.GetDailyRevenue();

        public List<DailyRevenueViewModel> GetDailyRevenueByDateRange(DateTime fromDate, DateTime toDate)
        {
            ValidateDateRange(fromDate, toDate);
            return _reportRepository.GetDailyRevenueByDateRange(fromDate.Date, toDate.Date);
        }

        public List<TopSellingBookViewModel> GetTopSellingBooks(int top = 10)
        {
            if (top <= 0)
            {
                top = 10;
            }

            if (top > 100)
            {
                top = 100;
            }

            return _reportRepository.GetTopSellingBooks(top);
        }

        private void ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                throw new Exception("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }
        }
    }
}
