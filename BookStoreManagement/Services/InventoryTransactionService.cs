using System;
using System.Collections.Generic;
using BookStoreManagement.Repositories;
using BookStoreManagement.Models;

namespace BookStoreManagement.Services
{
    public class InventoryTransactionService : ServiceBase
    {
        private readonly InventoryTransactionRepository _repo;

        public InventoryTransactionService()
        {
            _repo = new InventoryTransactionRepository();
        }

        public (List<InventoryTransactionDto> Items, int TotalCount) GetPagedTransactions(
            int page, int pageSize, DateTime fromDate, DateTime toDate,
            int bookId = 0, int storeId = 0, string referenceType = "")
        {
            return System.Threading.Tasks.Task.Run(() =>
                _repo.GetPagedTransactionsAsync(page, pageSize, fromDate, toDate, bookId, storeId, referenceType))
                .GetAwaiter().GetResult();
        }

        public async System.Threading.Tasks.Task<(List<InventoryTransactionDto> Items, int TotalCount)> GetPagedTransactionsAsync(
            int page, int pageSize, DateTime fromDate, DateTime toDate,
            int bookId = 0, int storeId = 0, string referenceType = "")
        {
            return await _repo.GetPagedTransactionsAsync(page, pageSize, fromDate, toDate, bookId, storeId, referenceType);
        }

        public async System.Threading.Tasks.Task<LedgerSummary> GetLedgerSummaryAsync(
            DateTime fromDate, DateTime toDate, int bookId = 0, int storeId = 0, string referenceType = "")
        {
            return await _repo.GetLedgerSummaryAsync(fromDate, toDate, bookId, storeId, referenceType);
        }

        public InventoryTransaction? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public bool UpdateNote(int id, string note)
        {
            return _repo.UpdateNote(id, note);
        }

        public bool Delete(int id)
        {
            return _repo.Delete(id);
        }

        public async System.Threading.Tasks.Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async System.Threading.Tasks.Task<bool> UpdateNoteAsync(int id, string note)
        {
            return await _repo.UpdateNoteAsync(id, note);
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
