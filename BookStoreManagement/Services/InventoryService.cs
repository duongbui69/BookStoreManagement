using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class InventoryService : ServiceBase
    {
        private readonly InventoryRepository _repository;

        public InventoryService()
        {
            _repository = new InventoryRepository();
        }

        public List<InventoryHistoryViewModel> GetHistory()
        {
            PermissionService.RequireAdmin();
            return _repository.GetHistory();
        }

        public List<InventoryHistoryViewModel> GetHistoryByStoreId(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            return _repository.GetHistoryByStoreId(resolvedStoreId);
        }

        public List<InventoryHistoryViewModel> GetHistoryByBookId(int bookId)
        {
            PermissionService.RequireAdmin();
            Require(bookId > 0, "Id sách không hợp lệ.");
            return _repository.GetHistoryByBookId(bookId);
        }

        public List<LowStockBookViewModel> GetLowStockBooks()
        {
            PermissionService.RequireAdmin();
            return _repository.GetLowStockBooks();
        }

        public List<LowStockBookViewModel> GetLowStockBooksByStoreId(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            return _repository.GetLowStockBooksByStoreId(resolvedStoreId);
        }

        public void AdjustStock(int storeId, int bookId, int quantityChange, string? note)
        {
            PermissionService.RequireAdmin();
            Require(storeId > 0, "Cửa hàng không hợp lệ.");
            Require(bookId > 0, "Sách không hợp lệ.");
            Require(quantityChange != 0, "Số lượng điều chỉnh không được bằng 0.");
            _repository.AdjustStock(storeId, bookId, CurrentSession.UserId, quantityChange, TrimNullable(note));
        }
    }
}
