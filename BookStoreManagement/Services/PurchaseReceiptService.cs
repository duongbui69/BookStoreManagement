using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            Require(storeId > 0, "Invalid store.");
            Require(supplierId > 0, "Invalid supplier.");
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

        public async Task<int> CreateReceiptAsync(int storeId, int supplierId, string? note, List<PurchaseReceiptDetail> details)
        {
            PermissionService.RequireAdmin();
            Require(storeId > 0, "Invalid store.");
            Require(supplierId > 0, "Invalid supplier.");
            ValidateDetails(details);

            string code = _repository.GenerateReceiptCode();
            while (await _repository.IsReceiptCodeExistsAsync(code)) code = _repository.GenerateReceiptCode();

            var receipt = new PurchaseReceipt
            {
                ReceiptCode = code,
                StoreId = storeId,
                SupplierId = supplierId,
                UserId = CurrentSession.UserId,
                Note = TrimNullable(note)
            };

            return await _repository.CreateReceiptAsync(receipt, details);
        }

        public List<PurchaseReceiptListViewModel> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public async Task<List<PurchaseReceiptListViewModel>> GetAllAsync() { PermissionService.RequireAdmin(); return await _repository.GetAllAsync(); }
        
        public PurchaseReceipt? GetById(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Invalid import receipt ID."); return _repository.GetById(id); }
        public async Task<PurchaseReceipt?> GetByIdAsync(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Invalid import receipt ID."); return await _repository.GetByIdAsync(id); }
        
        public List<PurchaseReceiptListViewModel> GetByStoreId(int storeId) { PermissionService.RequireAdmin(); Require(storeId > 0, "Invalid store."); return _repository.GetByStoreId(storeId); }
        public async Task<List<PurchaseReceiptListViewModel>> GetByStoreIdAsync(int storeId) { PermissionService.RequireAdmin(); Require(storeId > 0, "Invalid store."); return await _repository.GetByStoreIdAsync(storeId); }
        
        public List<PurchaseReceiptDetail> GetDetails(int receiptId) { PermissionService.RequireAdmin(); Require(receiptId > 0, "Invalid import receipt ID."); return _repository.GetDetails(receiptId); }
        public async Task<List<PurchaseReceiptDetail>> GetDetailsAsync(int receiptId) { PermissionService.RequireAdmin(); Require(receiptId > 0, "Invalid import receipt ID."); return await _repository.GetDetailsAsync(receiptId); }
        
        public List<PurchaseReceiptListViewModel> Search(string keyword, int? storeId = null) { PermissionService.RequireAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword, storeId); }
        public async Task<List<PurchaseReceiptListViewModel>> SearchAsync(string keyword, int? storeId = null) { PermissionService.RequireAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? await _repository.GetAllAsync() : await _repository.SearchAsync(keyword, storeId); }
        
        public List<PurchaseReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null) { PermissionService.RequireAdmin(); Require(fromDate <= toDate, "Invalid date range."); return _repository.GetByDateRange(fromDate, toDate, storeId); }
        public async Task<List<PurchaseReceiptListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? storeId = null) { PermissionService.RequireAdmin(); Require(fromDate <= toDate, "Invalid date range."); return await _repository.GetByDateRangeAsync(fromDate, toDate, storeId); }
        
        public (int TotalReceipts, decimal TotalValue, int PendingCount) GetStats() { PermissionService.RequireAdmin(); return _repository.GetStats(); }
        public async Task<(int TotalReceipts, decimal TotalValue, int PendingCount)> GetStatsAsync() { PermissionService.RequireAdmin(); return await _repository.GetStatsAsync(); }
        
        public void UpdateStatus(int id, string status) { PermissionService.RequireAdmin(); Require(id > 0, "Invalid import receipt ID."); Require(!string.IsNullOrWhiteSpace(status), "Invalid status."); _repository.UpdateStatus(id, status); }
        public async Task UpdateStatusAsync(int id, string status) { PermissionService.RequireAdmin(); Require(id > 0, "Invalid import receipt ID."); Require(!string.IsNullOrWhiteSpace(status), "Invalid status."); await _repository.UpdateStatusAsync(id, status); }

        private void ValidateDetails(List<PurchaseReceiptDetail> details)
        {
            Require(details != null && details.Count > 0, "Import receipt must have at least one book.");
            foreach (var detail in details)
            {
                Require(detail.BookId > 0, "Invalid book.");
                Require(detail.Quantity > 0, "Import quantity must be greater than 0.");
                Require(detail.ImportPrice >= 0, "Invalid import price.");
                if (detail.SellingPrice.HasValue) Require(detail.SellingPrice.Value >= 0, "Invalid selling price.");
            }
        }
    }
}
