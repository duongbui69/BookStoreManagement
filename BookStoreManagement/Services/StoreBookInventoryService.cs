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

        public System.Threading.Tasks.Task<List<StoreBookInventoryViewModel>> GetAllAsync()
        {
            PermissionService.RequireAdmin();
            return _repository.GetAllAsync();
        }

        public List<StoreBookInventoryViewModel> GetByStoreId(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            return _repository.GetByStoreId(resolvedStoreId);
        }

        public System.Threading.Tasks.Task<List<StoreBookInventoryViewModel>> GetByStoreIdAsync(int storeId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            return _repository.GetByStoreIdAsync(resolvedStoreId);
        }

        public StoreBookInventory? GetByStoreAndBook(int storeId, int bookId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            Require(bookId > 0, "Invalid book ID.");
            return _repository.GetByStoreAndBook(resolvedStoreId, bookId);
        }

        public System.Threading.Tasks.Task<StoreBookInventory?> GetByStoreAndBookAsync(int storeId, int bookId)
        {
            int resolvedStoreId = ResolveStoreIdForWrite(storeId);
            Require(bookId > 0, "Invalid book ID.");
            return _repository.GetByStoreAndBookAsync(resolvedStoreId, bookId);
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

        public System.Threading.Tasks.Task<List<StoreBookInventoryViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            int? resolvedStoreId = ResolveStoreIdForRead(storeId);
            keyword = Trim(keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return resolvedStoreId.HasValue ? _repository.GetByStoreIdAsync(resolvedStoreId.Value) : _repository.GetAllAsync();
            }
            return _repository.SearchAsync(keyword, resolvedStoreId);
        }

        public int Add(StoreBookInventory inventory)
        {
            PermissionService.RequireAdmin();
            Validate(inventory);
            inventory.IsActive = true;
            return _repository.Add(inventory);
        }

        public System.Threading.Tasks.Task<int> AddAsync(StoreBookInventory inventory)
        {
            PermissionService.RequireAdmin();
            Validate(inventory);
            inventory.IsActive = true;
            return _repository.AddAsync(inventory);
        }

        public bool Update(StoreBookInventory inventory)
        {
            PermissionService.RequireAdmin();
            Require(inventory.Id > 0, "Invalid inventory ID.");
            Validate(inventory);
            return _repository.Update(inventory);
        }

        public System.Threading.Tasks.Task<bool> UpdateAsync(StoreBookInventory inventory)
        {
            PermissionService.RequireAdmin();
            Require(inventory.Id > 0, "Invalid inventory ID.");
            Validate(inventory);
            return _repository.UpdateAsync(inventory);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Invalid inventory ID.");
            return _repository.SetActive(id, isActive);
        }

        public System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Invalid inventory ID.");
            return _repository.SetActiveAsync(id, isActive);
        }

        private void Validate(StoreBookInventory inventory)
        {
            Require(inventory != null, "Invalid inventory data.");
            Require(inventory.StoreId > 0, "Invalid store.");
            Require(inventory.BookId > 0, "Invalid book.");
            Require(inventory.Quantity >= 0, "Invalid stock quantity.");
            Require(inventory.MinStock >= 0, "Invalid min stock.");
            Require(inventory.SellingPrice >= 0, "Invalid selling price.");
            if (inventory.ImportPrice.HasValue) Require(inventory.ImportPrice.Value >= 0, "Invalid import price.");
        }
    }
}
