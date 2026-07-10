using System;
using System.Collections.Generic;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class ReportService : ServiceBase
    {
        private readonly ReportRepository _repository;

        public ReportService()
        {
            _repository = new ReportRepository();
        }

        public decimal GetTodayRevenue(int? storeId = null)
        {
            PermissionService.RequireAdmin();
            return _repository.GetTodayRevenue(storeId);
        }

        public decimal GetMonthRevenue(int? storeId = null)
        {
            PermissionService.RequireAdmin();
            return _repository.GetMonthRevenue(storeId);
        }

        public int GetTotalOrders(int? storeId = null)
        {
            PermissionService.RequireAdmin();
            return _repository.GetTotalOrders(storeId);
        }

        public int GetTotalBooks()
        {
            PermissionService.RequireAdmin();
            return _repository.GetTotalBooks();
        }

        public int GetTotalUsers()
        {
            PermissionService.RequireAdmin();
            return _repository.GetTotalUsers();
        }

        public int GetTotalCustomers()
        {
            PermissionService.RequireAdmin();
            return _repository.GetTotalCustomers();
        }

        public int GetTotalStores()
        {
            PermissionService.RequireAdmin();
            return _repository.GetTotalStores();
        }

        public List<DailyRevenueViewModel> GetDailyRevenue()
        {
            PermissionService.RequireAdmin();
            return _repository.GetDailyRevenue();
        }

        public List<DailyRevenueByStoreViewModel> GetDailyRevenueByStore(int? storeId = null)
        {
            PermissionService.RequireAdmin();
            return _repository.GetDailyRevenueByStore(storeId);
        }

        public List<TopSellingBookViewModel> GetTopSellingBooks(int top = 10)
        {
            PermissionService.RequireAdmin();
            if (top <= 0) top = 10;
            return _repository.GetTopSellingBooks(top);
        }
    }
}
