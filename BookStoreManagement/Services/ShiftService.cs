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
            return _shiftRepository.GetActiveShift(CurrentSession.UserId, CurrentSession.StoreId ?? 1);
        }

        public void OpenShift(string shiftName, decimal initialCash)
        {
            var activeShift = GetActiveShift();
            if (activeShift != null)
            {
                throw new Exception("You have an open shift. Please close it before opening a new one.");
            }

            var shift = new Shift
            {
                StaffId = CurrentSession.UserId,
                StoreId = CurrentSession.StoreId ?? 1, // Default to store 1 if user doesn't have a specific store
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
                throw new Exception("Invalid or closed shift.");
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
                    totalRevenue += order.ActualAmount;
                    totalOrdersCount++;
                }
            }

            _shiftRepository.CloseShift(shiftId, totalRevenue, totalOrdersCount);
        }

        public List<ShiftViewModel> GetShiftHistory()
        {
            return _shiftRepository.GetShiftHistory(CurrentSession.UserId, CurrentSession.StoreId ?? 1);
        }

        public List<ShiftViewModel> GetShiftHistoryByStaff(int staffId, int storeId)
        {
            return _shiftRepository.GetShiftHistory(staffId, storeId);
        }
    
        public Shift? GetShiftById(int id)
        {
            return _shiftRepository.GetShiftById(id);
        }

        public List<ShiftViewModel> GetAllShifts()
        {
            return _shiftRepository.GetAllShifts();
        }
}
}
