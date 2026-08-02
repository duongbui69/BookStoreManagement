using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class ReturnReceiptService : ServiceBase
    {
        private readonly ReturnReceiptRepository _repository;

        public ReturnReceiptService()
        {
            _repository = new ReturnReceiptRepository();
        }

        public int CreateReturn(int? salesOrderId, int storeId, int? customerId, string? note, List<ReturnReceiptDetail> details)
        {
            PermissionService.RequireStaffOrAdmin();
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            ValidateDetails(details);

            string code = _repository.GenerateReturnCode();
            while (_repository.IsReturnCodeExists(code)) code = _repository.GenerateReturnCode();

            decimal totalRefund = 0;
            if (details != null)
            {
                foreach (var detail in details) totalRefund += detail.Quantity * detail.UnitPrice;
            }

            var receipt = new ReturnReceipt
            {
                ReturnCode = code,
                SalesOrderId = salesOrderId,
                StoreId = resolvedStoreId,
                CustomerId = customerId,
                UserId = CurrentSession.UserId,
                TotalRefundAmount = totalRefund,
                ReturnStatus = "Processing",
                Note = TrimNullable(note)
            };

            return _repository.CreateReturn(receipt, details);
        }

        public List<ReturnReceiptListViewModel> GetAll()
        {
            PermissionService.RequireAdmin();
            return _repository.GetAll();
        }

        public List<ReturnReceiptListViewModel> GetVisibleReturns()
        {
            PermissionService.RequireStaffOrAdmin();
            if (CurrentSession.IsAdmin) return _repository.GetAll();
            return _repository.GetByStoreId(CurrentSession.StoreId!.Value);
        }

        public ReturnReceipt? GetById(int id)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(id > 0, "Invalid return receipt ID.");
            ReturnReceipt? receipt = _repository.GetById(id);
            if (receipt != null) PermissionService.RequireSameStoreOrAdmin(receipt.StoreId);
            return receipt;
        }

        public List<ReturnReceiptDetailFullViewModel> GetDetails(int returnReceiptId)
        {
            ReturnReceipt? receipt = GetById(returnReceiptId);
            Require(receipt != null, "Return receipt not found.");
            return _repository.GetDetails(returnReceiptId);
        }

        public List<ReturnReceiptListViewModel> Search(string keyword, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            keyword = Trim(keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return resolvedStoreId.HasValue ? _repository.GetByStoreId(resolvedStoreId.Value) : _repository.GetAll();
            }
            return _repository.Search(keyword, resolvedStoreId);
        }

        public List<ReturnReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(fromDate <= toDate, "Invalid date range.");
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            return _repository.GetByDateRange(fromDate, toDate, resolvedStoreId);
        }

        private void ValidateDetails(List<ReturnReceiptDetail> details)
        {
            Require(details != null && details.Count > 0, "Return receipt must have at least one book.");
            foreach (var detail in details)
            {
                Require(detail.BookId > 0, "Invalid book.");
                Require(detail.Quantity > 0, "Return quantity must be greater than 0.");
                Require(detail.UnitPrice >= 0, "Invalid refund unit price.");
                detail.ReturnReason = TrimNullable(detail.ReturnReason);
            }
        }

        public async System.Threading.Tasks.Task<int> CreateReturnAsync(int? salesOrderId, int storeId, int? customerId, string? note, List<ReturnReceiptDetail> details)
        {
            PermissionService.RequireStaffOrAdmin();
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            ValidateDetails(details);

            string code = _repository.GenerateReturnCode();
            while (await _repository.IsReturnCodeExistsAsync(code)) code = _repository.GenerateReturnCode();

            decimal totalRefund = 0;
            if (details != null)
            {
                foreach (var detail in details) totalRefund += detail.Quantity * detail.UnitPrice;
            }

            var receipt = new ReturnReceipt
            {
                ReturnCode = code,
                SalesOrderId = salesOrderId,
                StoreId = resolvedStoreId,
                CustomerId = customerId,
                UserId = CurrentSession.UserId,
                TotalRefundAmount = totalRefund,
                ReturnStatus = "Processing",
                Note = TrimNullable(note)
            };

            return await _repository.CreateReturnAsync(receipt, details);
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> GetAllAsync()
        {
            PermissionService.RequireAdmin();
            return await _repository.GetAllAsync();
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> GetVisibleReturnsAsync()
        {
            PermissionService.RequireStaffOrAdmin();
            if (CurrentSession.IsAdmin) return await _repository.GetAllAsync();
            return await _repository.GetByStoreIdAsync(CurrentSession.StoreId!.Value);
        }

        public async System.Threading.Tasks.Task<ReturnReceipt?> GetByIdAsync(int id)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(id > 0, "Invalid return receipt ID.");
            ReturnReceipt? receipt = await _repository.GetByIdAsync(id);
            if (receipt != null) PermissionService.RequireSameStoreOrAdmin(receipt.StoreId);
            return receipt;
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptDetailFullViewModel>> GetDetailsAsync(int returnReceiptId)
        {
            ReturnReceipt? receipt = await GetByIdAsync(returnReceiptId);
            Require(receipt != null, "Return receipt not found.");
            return await _repository.GetDetailsAsync(returnReceiptId);
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            keyword = Trim(keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return resolvedStoreId.HasValue ? await _repository.GetByStoreIdAsync(resolvedStoreId.Value) : await _repository.GetAllAsync();
            }
            return await _repository.SearchAsync(keyword, resolvedStoreId);
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(fromDate <= toDate, "Invalid date range.");
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            return await _repository.GetByDateRangeAsync(fromDate, toDate, resolvedStoreId);
        }
    }
}
