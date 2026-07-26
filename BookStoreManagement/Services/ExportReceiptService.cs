using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class ExportReceiptService : ServiceBase
    {
        private readonly ExportReceiptRepository _repository;
        public ExportReceiptService() { _repository = new ExportReceiptRepository(); }

        public int CreateReceipt(int storeId, string reason, string? customerName, string? note, List<ExportReceiptDetail> details)
        {
            PermissionService.RequireAdmin();
            Require(storeId > 0, "Vui lòng chọn Kho xuất.");
            Require(details != null && details.Count > 0, "Phiếu xuất phải có ít nhất một sản phẩm.");

            var receipt = new ExportReceipt
            {
                ReceiptCode = "EX" + DateTime.Now.ToString("yyMMddHHmmss"),
                StoreId = storeId,
                UserId = CurrentSession.UserId > 0 ? CurrentSession.UserId : 1,
                Reason = reason,
                CustomerName = customerName ?? string.Empty,
                ExportDate = DateTime.Now,
                TotalAmount = details.Sum(d => d.LineTotal),
                Status = "Hoàn thành",
                Note = note
            };

            return ((ExportReceiptRepository)_repository).CreateReceipt(receipt, details);
        }

        public async System.Threading.Tasks.Task<int> CreateReceiptAsync(int storeId, string reason, string? customerName, string? note, List<ExportReceiptDetail> details)
        {
            PermissionService.RequireAdmin();
            Require(storeId > 0, "Vui lòng chọn Kho xuất.");
            Require(details != null && details.Count > 0, "Phiếu xuất phải có ít nhất một sản phẩm.");

            var receipt = new ExportReceipt
            {
                ReceiptCode = "EX" + DateTime.Now.ToString("yyMMddHHmmss"),
                StoreId = storeId,
                UserId = CurrentSession.UserId > 0 ? CurrentSession.UserId : 1,
                Reason = reason,
                CustomerName = customerName ?? string.Empty,
                ExportDate = DateTime.Now,
                TotalAmount = details.Sum(d => d.LineTotal),
                Status = "Hoàn thành",
                Note = note
            };

            return await ((ExportReceiptRepository)_repository).CreateReceiptAsync(receipt, details);
        }

        public List<ExportReceiptListViewModel> GetAll() 
        { 
            return _repository.GetAll(); 
        }

        public async System.Threading.Tasks.Task<List<ExportReceiptListViewModel>> GetAllAsync() 
        { 
            return await _repository.GetAllAsync(); 
        }

        public ExportReceipt? GetById(int id) 
        { 
            Require(id > 0, "Invalid export receipt ID."); 
            return _repository.GetById(id); 
        }

        public async System.Threading.Tasks.Task<ExportReceipt?> GetByIdAsync(int id) 
        { 
            Require(id > 0, "Invalid export receipt ID."); 
            return await _repository.GetByIdAsync(id); 
        }

        public List<ExportReceiptDetail> GetDetails(int receiptId) 
        { 
            Require(receiptId > 0, "Invalid export receipt ID."); 
            return _repository.GetDetails(receiptId); 
        }

        public async System.Threading.Tasks.Task<List<ExportReceiptDetail>> GetDetailsAsync(int receiptId) 
        { 
            Require(receiptId > 0, "Invalid export receipt ID."); 
            return await _repository.GetDetailsAsync(receiptId); 
        }

        public (int TotalReceipts, decimal TotalValue, int PendingCount) GetStats() 
        { 
            return _repository.GetStats(); 
        }

        public async System.Threading.Tasks.Task<(int TotalReceipts, decimal TotalValue, int PendingCount)> GetStatsAsync() 
        { 
            return await _repository.GetStatsAsync(); 
        }

        public void UpdateStatus(int id, string status) 
        { 
            Require(id > 0, "Invalid export receipt ID."); 
            Require(!string.IsNullOrWhiteSpace(status), "Invalid status."); 
            _repository.UpdateStatus(id, status); 
        }

        public async System.Threading.Tasks.Task UpdateStatusAsync(int id, string status) 
        { 
            Require(id > 0, "Invalid export receipt ID."); 
            Require(!string.IsNullOrWhiteSpace(status), "Invalid status."); 
            await _repository.UpdateStatusAsync(id, status); 
        }
    }
}
