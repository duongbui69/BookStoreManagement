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

            var receipt = new ReturnReceipt
            {
                ReturnCode = code,
                SalesOrderId = salesOrderId,
                StoreId = resolvedStoreId,
                CustomerId = customerId,
                UserId = CurrentSession.UserId,
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
            Require(id > 0, "Id phiếu trả không hợp lệ.");
            ReturnReceipt? receipt = _repository.GetById(id);
            if (receipt != null) PermissionService.RequireSameStoreOrAdmin(receipt.StoreId);
            return receipt;
        }

        public List<ReturnReceiptDetailFullViewModel> GetDetails(int returnReceiptId)
        {
            ReturnReceipt? receipt = GetById(returnReceiptId);
            Require(receipt != null, "Không tìm thấy phiếu trả.");
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
            Require(fromDate <= toDate, "Khoảng ngày không hợp lệ.");
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            return _repository.GetByDateRange(fromDate, toDate, resolvedStoreId);
        }

        private void ValidateDetails(List<ReturnReceiptDetail> details)
        {
            Require(details != null && details.Count > 0, "Phiếu trả phải có ít nhất một sách.");
            foreach (var detail in details)
            {
                Require(detail.BookId > 0, "Sách không hợp lệ.");
                Require(detail.Quantity > 0, "Số lượng trả phải lớn hơn 0.");
                Require(detail.UnitPrice >= 0, "Đơn giá hoàn không hợp lệ.");
                detail.ReturnReason = TrimNullable(detail.ReturnReason);
            }
        }
    }
}
