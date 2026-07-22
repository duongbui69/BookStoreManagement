using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class ShiftService
    {
        private readonly ShiftRepository _shiftRepository;
        private readonly SalesOrderRepository _salesOrderRepository;

        public ShiftService()
        {
            _shiftRepository = new ShiftRepository();
            _salesOrderRepository = new SalesOrderRepository();
        }

        public Shift? GetActiveShift()
        {
            return _shiftRepository.GetActiveShift(CurrentSession.UserId, CurrentSession.StoreId ?? 0);
        }

        public void OpenShift(string shiftName, decimal initialCash)
        {
            var activeShift = GetActiveShift();
            if (activeShift != null)
            {
                throw new Exception("Bạn đang có một ca làm việc mở. Vui lòng chốt ca trước khi mở ca mới.");
            }

            var shift = new Shift
            {
                StaffId = CurrentSession.UserId,
                StoreId = CurrentSession.StoreId ?? 0,
                ShiftName = shiftName,
                StartTime = DateTime.Now,
                InitialCash = initialCash
            };

            _shiftRepository.OpenShift(shift);
        }

        public void CloseShift(int shiftId)
        {
            var activeShift = GetActiveShift();
            if (activeShift == null || activeShift.Id != shiftId)
            {
                throw new Exception("Ca làm việc không hợp lệ hoặc đã đóng.");
            }

            // Calculate actual revenue and orders during this shift
            var orders = _salesOrderRepository.GetByDateRange(activeShift.StartTime, DateTime.Now);
            
            // Filter orders for current staff only
            decimal totalRevenue = 0;
            int totalOrdersCount = 0;

            foreach (var order in orders)
            {
                if (order.StaffId == CurrentSession.UserId)
                {
                    totalRevenue += order.TotalAmount;
                    totalOrdersCount++;
                }
            }

            _shiftRepository.CloseShift(shiftId, totalRevenue, totalOrdersCount);
        }

        public List<ShiftViewModel> GetShiftHistory()
        {
            return _shiftRepository.GetShiftHistory(CurrentSession.UserId, CurrentSession.StoreId ?? 0);
        }
    }
}
