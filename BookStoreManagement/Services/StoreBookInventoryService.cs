using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class StoreBookInventoryService : ServiceBase
    {
        private readonly StoreBookInventoryRepository _repository;
        public StoreBookInventoryService() { _repository = new StoreBookInventoryRepository(); }

        public List<StoreBookInventoryViewModel> GetAll()
        {
            PermissionService.RequireAdmin();
            return _repository.GetAll();
        }

        public List<StoreBookInventoryViewModel> GetByStoreId(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            return _repository.GetByStoreId(resolvedStoreId);
        }

        public StoreBookInventory? GetByStoreAndBook(int storeId, int bookId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            Require(bookId > 0, "Id sách không hợp lệ.");
            return _repository.GetByStoreAndBook(resolvedStoreId, bookId);
        }

        public List<StoreBookInventoryViewModel> Search(string keyword, int? storeId = null)
        {
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            keyword = Trim(keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return resolvedStoreId.HasValue ? _repository.GetByStoreId(resolvedStoreId.Value) : _repository.GetAll();
            }
            return _repository.Search(keyword, resolvedStoreId);
        }

        public int Add(StoreBookInventory inventory)
        {
            PermissionService.RequireAdmin();
            Validate(inventory);
            inventory.IsActive = true;
            return _repository.Add(inventory);
        }

        public bool Update(StoreBookInventory inventory)
        {
            PermissionService.RequireAdmin();
            Require(inventory.Id > 0, "Id tồn kho không hợp lệ.");
            Validate(inventory);
            return _repository.Update(inventory);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id tồn kho không hợp lệ.");
            return _repository.SetActive(id, isActive);
        }

        private void Validate(StoreBookInventory inventory)
        {
            Require(inventory != null, "Dữ liệu tồn kho không hợp lệ.");
            Require(inventory.StoreId > 0, "Cửa hàng không hợp lệ.");
            Require(inventory.BookId > 0, "Sách không hợp lệ.");
            Require(inventory.Quantity >= 0, "Số lượng tồn không hợp lệ.");
            Require(inventory.MinStock >= 0, "Tồn tối thiểu không hợp lệ.");
            Require(inventory.SellingPrice >= 0, "Giá bán không hợp lệ.");
            if (inventory.ImportPrice.HasValue) Require(inventory.ImportPrice.Value >= 0, "Giá nhập không hợp lệ.");
        }
    }
}
