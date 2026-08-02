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

            decimal totalAmount = 0;
            if (details != null)
            {
                foreach (var detail in details) totalAmount += detail.LineTotal;
            }

            var order = new SalesOrder
            {
                OrderCode = orderCode,
                StoreId = resolvedStoreId,
                UserId = CurrentSession.UserId,
                CustomerId = customerId,
                TotalAmount = totalAmount,
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
            Require(id > 0, "Invalid invoice ID.");
            SalesOrder? order = _salesOrderRepository.GetById(id);
            if (order != null && !CurrentSession.IsAdmin && order.UserId != CurrentSession.UserId) throw new Exception("You can only view invoices you created.");
            return order;
        }

        public List<SalesOrderDetailFullViewModel> GetDetails(int salesOrderId)
        {
            SalesOrder? order = GetById(salesOrderId);
            Require(order != null, "Invoice not found.");
            return _salesOrderRepository.GetDetails(salesOrderId);
        }

        public List<SalesOrderListViewModel> Search(string keyword, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            keyword = Trim(keyword);
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            
            var orders = string.IsNullOrWhiteSpace(keyword) ? _salesOrderRepository.GetAll() : _salesOrderRepository.Search(keyword, resolvedStoreId);
            
            if (CurrentSession.IsStaff)
            {
                orders = orders.Where(x => x.StaffId == CurrentSession.UserId).ToList();
            }
            return orders;
        }

        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            var orders = _salesOrderRepository.GetByDateRange(fromDate, toDate);
            
            if (CurrentSession.IsStaff)
            {
                orders = orders.Where(x => x.StaffId == CurrentSession.UserId).ToList();
            }
            else if (storeId.HasValue)
            {
                orders = orders.Where(x => x.StoreId == storeId.Value).ToList();
            }
            return orders;
        }

        public bool CancelOrder(int salesOrderId)
        {
            PermissionService.RequireAdmin();
            Require(salesOrderId > 0, "Invalid invoice ID.");
            return _salesOrderRepository.CancelOrder(salesOrderId);
        }

        private void ValidateDetails(int storeId, List<SalesOrderDetail> details)
        {
            Require(details != null && details.Count > 0, "Invoice must contain at least one book.");
            foreach (var detail in details)
            {
                Require(detail.BookId > 0, "Invalid book.");
                Require(detail.Quantity > 0, "Sales quantity must be greater than 0.");
                Require(detail.UnitPrice >= 0, "Invalid unit price.");
                Require(detail.DiscountAmount >= 0, "Invalid discount.");
                bool hasStock = _bookRepository.HasEnoughStock(storeId, detail.BookId, detail.Quantity);
                if (!hasStock) throw new Exception("Not enough stock to sell.");
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

            decimal totalAmount = 0;
            if (details != null)
            {
                foreach (var detail in details) totalAmount += detail.LineTotal;
            }

            var order = new SalesOrder
            {
                OrderCode = orderCode,
                StoreId = resolvedStoreId,
                UserId = CurrentSession.UserId,
                CustomerId = customerId,
                TotalAmount = totalAmount,
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
            Require(id > 0, "Invalid invoice ID.");
            SalesOrder? order = await _salesOrderRepository.GetByIdAsync(id);
            if (order != null && !CurrentSession.IsAdmin && order.UserId != CurrentSession.UserId) throw new Exception("You can only view invoices you created.");
            return order;
        }

        public async Task<List<SalesOrderDetailFullViewModel>> GetDetailsAsync(int salesOrderId)
        {
            SalesOrder? order = await GetByIdAsync(salesOrderId);
            Require(order != null, "Invoice not found.");
            return await _salesOrderRepository.GetDetailsAsync(salesOrderId);
        }

        public async Task<List<SalesOrderListViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            keyword = Trim(keyword);
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            
            var orders = string.IsNullOrWhiteSpace(keyword) ? await _salesOrderRepository.GetAllAsync() : await _salesOrderRepository.SearchAsync(keyword, resolvedStoreId);
            if (CurrentSession.IsStaff)
            {
                orders = orders.Where(x => x.StaffId == CurrentSession.UserId).ToList();
            }
            return orders;
        }

        public async Task<List<SalesOrderListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            var orders = await _salesOrderRepository.GetByDateRangeAsync(fromDate, toDate);
            if (CurrentSession.IsStaff)
            {
                orders = orders.Where(x => x.StaffId == CurrentSession.UserId).ToList();
            }
            else if (storeId.HasValue)
            {
                orders = orders.Where(x => x.StoreId == storeId.Value).ToList();
            }
            return orders;
        }

        public async Task<bool> CancelOrderAsync(int salesOrderId)
        {
            PermissionService.RequireAdmin();
            Require(salesOrderId > 0, "Invalid invoice ID.");
            return await _salesOrderRepository.CancelOrderAsync(salesOrderId);
        }
    }
}
