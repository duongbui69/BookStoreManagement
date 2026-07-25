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
