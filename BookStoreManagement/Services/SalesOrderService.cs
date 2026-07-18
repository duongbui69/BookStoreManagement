using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class SalesOrderService : ServiceBase
    {
        private readonly SalesOrderRepository _salesOrderRepository;
        private readonly BookRepository _bookRepository;

        public SalesOrderService()
        {
            _salesOrderRepository = new SalesOrderRepository();
            _bookRepository = new BookRepository();
        }

        public SalesOrder? GetByOrderCode(string orderCode)
        {
            return _salesOrderRepository.GetByOrderCode(orderCode);
        }

        public int CreateOrder(int storeId, int? customerId, string paymentMethod, string? note, List<SalesOrderDetail> details)
        {
            PermissionService.RequireStaffOrAdmin();
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            ValidateDetails(resolvedStoreId, details);

            string orderCode = _salesOrderRepository.GenerateOrderCode();
            while (_salesOrderRepository.IsOrderCodeExists(orderCode)) orderCode = _salesOrderRepository.GenerateOrderCode();

            var order = new SalesOrder
            {
                OrderCode = orderCode,
                StoreId = resolvedStoreId,
                UserId = CurrentSession.UserId,
                CustomerId = customerId,
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? AppConstants.PaymentMethods.Cash : paymentMethod.Trim(),
                OrderStatus = AppConstants.OrderStatuses.Completed,
                Note = TrimNullable(note)
            };

            return _salesOrderRepository.CreateOrder(order, details);
        }

        public List<SalesOrderListViewModel> GetAll()
        {
            PermissionService.RequireAdmin();
            return _salesOrderRepository.GetAll();
        }

        public List<SalesOrderListViewModel> GetVisibleOrders()
        {
            PermissionService.RequireStaffOrAdmin();
            if (CurrentSession.IsAdmin) return _salesOrderRepository.GetAll();
            return _salesOrderRepository.GetByStaffId(CurrentSession.UserId);
        }

        public List<SalesOrderListViewModel> GetByStoreId(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            if (CurrentSession.IsAdmin) return _salesOrderRepository.GetByStoreId(resolvedStoreId);
            return _salesOrderRepository.GetByStaffId(CurrentSession.UserId);
        }

        public List<SalesOrderListViewModel> GetMyOrders()
        {
            PermissionService.RequireStaffOrAdmin();
            return _salesOrderRepository.GetByStaffId(CurrentSession.UserId);
        }

        public SalesOrder? GetById(int id)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(id > 0, "Id hóa đơn không hợp lệ.");
            SalesOrder? order = _salesOrderRepository.GetById(id);
            if (order != null && !CurrentSession.IsAdmin && order.UserId != CurrentSession.UserId) throw new Exception("Bạn chỉ được xem hóa đơn do mình lập.");
            return order;
        }

        public List<SalesOrderDetailFullViewModel> GetDetails(int salesOrderId)
        {
            SalesOrder? order = GetById(salesOrderId);
            Require(order != null, "Không tìm thấy hóa đơn.");
            return _salesOrderRepository.GetDetails(salesOrderId);
        }

        public List<SalesOrderListViewModel> Search(string keyword, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            keyword = Trim(keyword);
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            if (CurrentSession.IsStaff) return _salesOrderRepository.GetByStaffId(CurrentSession.UserId);
            return string.IsNullOrWhiteSpace(keyword) ? _salesOrderRepository.GetAll() : _salesOrderRepository.Search(keyword, resolvedStoreId);
        }

        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            if (CurrentSession.IsStaff) return _salesOrderRepository.GetByStaffId(CurrentSession.UserId);
            return _salesOrderRepository.GetByDateRange(fromDate, toDate);
        }

        public bool CancelOrder(int salesOrderId)
        {
            PermissionService.RequireAdmin();
            Require(salesOrderId > 0, "Id hóa đơn không hợp lệ.");
            return _salesOrderRepository.CancelOrder(salesOrderId);
        }

        private void ValidateDetails(int storeId, List<SalesOrderDetail> details)
        {
            Require(details != null && details.Count > 0, "Hóa đơn phải có ít nhất một sách.");
            foreach (var detail in details)
            {
                Require(detail.BookId > 0, "Sách không hợp lệ.");
                Require(detail.Quantity > 0, "Số lượng bán phải lớn hơn 0.");
                Require(detail.UnitPrice >= 0, "Đơn giá không hợp lệ.");
                Require(detail.DiscountAmount >= 0, "Giảm giá không hợp lệ.");
                bool hasStock = _bookRepository.HasEnoughStock(storeId, detail.BookId, detail.Quantity);
                if (!hasStock) throw new Exception("Sách không đủ tồn kho để bán.");
            }
        }

        public async Task<SalesOrder?> GetByOrderCodeAsync(string orderCode)
        {
            return await _salesOrderRepository.GetByOrderCodeAsync(orderCode);
        }

        public async Task<int> CreateOrderAsync(int storeId, int? customerId, string paymentMethod, string? note, List<SalesOrderDetail> details)
        {
            PermissionService.RequireStaffOrAdmin();
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            ValidateDetails(resolvedStoreId, details);

            string orderCode = await _salesOrderRepository.GenerateOrderCodeAsync();
            while (await _salesOrderRepository.IsOrderCodeExistsAsync(orderCode)) orderCode = await _salesOrderRepository.GenerateOrderCodeAsync();

            var order = new SalesOrder
            {
                OrderCode = orderCode,
                StoreId = resolvedStoreId,
                UserId = CurrentSession.UserId,
                CustomerId = customerId,
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? AppConstants.PaymentMethods.Cash : paymentMethod.Trim(),
                OrderStatus = AppConstants.OrderStatuses.Completed,
                Note = TrimNullable(note)
            };

            return await _salesOrderRepository.CreateOrderAsync(order, details);
        }

        public async Task<List<SalesOrderListViewModel>> GetAllAsync()
        {
            PermissionService.RequireAdmin();
            return await _salesOrderRepository.GetAllAsync();
        }

        public async Task<List<SalesOrderListViewModel>> GetVisibleOrdersAsync()
        {
            PermissionService.RequireStaffOrAdmin();
            if (CurrentSession.IsAdmin) return await _salesOrderRepository.GetAllAsync();
            return await _salesOrderRepository.GetByStaffIdAsync(CurrentSession.UserId);
        }

        public async Task<List<SalesOrderListViewModel>> GetByStoreIdAsync(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            if (CurrentSession.IsAdmin) return await _salesOrderRepository.GetByStoreIdAsync(resolvedStoreId);
            return await _salesOrderRepository.GetByStaffIdAsync(CurrentSession.UserId);
        }

        public async Task<List<SalesOrderListViewModel>> GetMyOrdersAsync()
        {
            PermissionService.RequireStaffOrAdmin();
            return await _salesOrderRepository.GetByStaffIdAsync(CurrentSession.UserId);
        }

        public async Task<SalesOrder?> GetByIdAsync(int id)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(id > 0, "Id hóa đơn không hợp lệ.");
            SalesOrder? order = await _salesOrderRepository.GetByIdAsync(id);
            if (order != null && !CurrentSession.IsAdmin && order.UserId != CurrentSession.UserId) throw new Exception("Bạn chỉ được xem hóa đơn do mình lập.");
            return order;
        }

        public async Task<List<SalesOrderDetailFullViewModel>> GetDetailsAsync(int salesOrderId)
        {
            SalesOrder? order = await GetByIdAsync(salesOrderId);
            Require(order != null, "Không tìm thấy hóa đơn.");
            return await _salesOrderRepository.GetDetailsAsync(salesOrderId);
        }

        public async Task<List<SalesOrderListViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            keyword = Trim(keyword);
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            if (CurrentSession.IsStaff) return await _salesOrderRepository.GetByStaffIdAsync(CurrentSession.UserId);
            return string.IsNullOrWhiteSpace(keyword) ? await _salesOrderRepository.GetAllAsync() : await _salesOrderRepository.SearchAsync(keyword, resolvedStoreId);
        }

        public async Task<List<SalesOrderListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            if (CurrentSession.IsStaff) return await _salesOrderRepository.GetByStaffIdAsync(CurrentSession.UserId);
            return await _salesOrderRepository.GetByDateRangeAsync(fromDate, toDate);
        }

        public async Task<bool> CancelOrderAsync(int salesOrderId)
        {
            PermissionService.RequireAdmin();
            Require(salesOrderId > 0, "Id hóa đơn không hợp lệ.");
            return await _salesOrderRepository.CancelOrderAsync(salesOrderId);
        }
    }
}
