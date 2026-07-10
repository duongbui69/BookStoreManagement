using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class PurchaseReceiptService : ServiceBase
    {
        private readonly PurchaseReceiptRepository _repository;
        public PurchaseReceiptService() { _repository = new PurchaseReceiptRepository(); }

        public int CreateReceipt(int storeId, int supplierId, string? note, List<PurchaseReceiptDetail> details)
        {
            PermissionService.RequireAdmin();
            Require(storeId > 0, "Cửa hàng không hợp lệ.");
            Require(supplierId > 0, "Nhà cung cấp không hợp lệ.");
            ValidateDetails(details);

            string code = _repository.GenerateReceiptCode();
            while (_repository.IsReceiptCodeExists(code)) code = _repository.GenerateReceiptCode();

            var receipt = new PurchaseReceipt
            {
                ReceiptCode = code,
                StoreId = storeId,
                SupplierId = supplierId,
                UserId = CurrentSession.UserId,
                Note = TrimNullable(note)
            };

            return _repository.CreateReceipt(receipt, details);
        }

        public List<PurchaseReceiptListViewModel> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public PurchaseReceipt? GetById(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Id phiếu nhập không hợp lệ."); return _repository.GetById(id); }
        public List<PurchaseReceiptListViewModel> GetByStoreId(int storeId) { PermissionService.RequireAdmin(); Require(storeId > 0, "Cửa hàng không hợp lệ."); return _repository.GetByStoreId(storeId); }
        public List<PurchaseReceiptDetail> GetDetails(int receiptId) { PermissionService.RequireAdmin(); Require(receiptId > 0, "Id phiếu nhập không hợp lệ."); return _repository.GetDetails(receiptId); }
        public List<PurchaseReceiptListViewModel> Search(string keyword, int? storeId = null) { PermissionService.RequireAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword, storeId); }
        public List<PurchaseReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null) { PermissionService.RequireAdmin(); Require(fromDate <= toDate, "Khoảng ngày không hợp lệ."); return _repository.GetByDateRange(fromDate, toDate, storeId); }

        private void ValidateDetails(List<PurchaseReceiptDetail> details)
        {
            Require(details != null && details.Count > 0, "Phiếu nhập phải có ít nhất một sách.");
            foreach (var detail in details)
            {
                Require(detail.BookId > 0, "Sách không hợp lệ.");
                Require(detail.Quantity > 0, "Số lượng nhập phải lớn hơn 0.");
                Require(detail.ImportPrice >= 0, "Giá nhập không hợp lệ.");
                if (detail.SellingPrice.HasValue) Require(detail.SellingPrice.Value >= 0, "Giá bán không hợp lệ.");
            }
        }
    }
}
