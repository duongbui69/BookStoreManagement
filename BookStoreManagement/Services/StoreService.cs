using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class StoreService : ServiceBase
    {
        private readonly StoreRepository _storeRepository;

        public StoreService()
        {
            _storeRepository = new StoreRepository();
        }

        public List<Store> GetAll()
        {
            PermissionService.RequireAdmin();
            return _storeRepository.GetAll();
        }

        public async System.Threading.Tasks.Task<List<Store>> GetAllAsync()
        {
            PermissionService.RequireAdmin();
            return await _storeRepository.GetAllAsync();
        }

        public List<Store> GetActive()
        {
            PermissionService.RequireStaffOrAdmin();
            return _storeRepository.GetActive();
        }

        public async System.Threading.Tasks.Task<List<Store>> GetActiveAsync()
        {
            PermissionService.RequireStaffOrAdmin();
            return await _storeRepository.GetActiveAsync();
        }

        public Store? GetById(int id)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(id > 0, "Id cửa hàng không hợp lệ.");
            return _storeRepository.GetById(id);
        }

        public async System.Threading.Tasks.Task<Store?> GetByIdAsync(int id)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(id > 0, "Id cửa hàng không hợp lệ.");
            return await _storeRepository.GetByIdAsync(id);
        }

        public List<Store> Search(string keyword)
        {
            PermissionService.RequireAdmin();
            keyword = Trim(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? _storeRepository.GetAll() : _storeRepository.Search(keyword);
        }

        public async System.Threading.Tasks.Task<List<Store>> SearchAsync(string keyword)
        {
            PermissionService.RequireAdmin();
            keyword = Trim(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? await _storeRepository.GetAllAsync() : await _storeRepository.SearchAsync(keyword);
        }

        public int Add(Store store)
        {
            PermissionService.RequireAdmin();
            Validate(store, false);

            if (_storeRepository.IsStoreCodeExists(store.StoreCode))
                throw new Exception("Mã cửa hàng đã tồn tại.");
            if (_storeRepository.IsStoreNameExists(store.StoreName))
                throw new Exception("Tên cửa hàng đã tồn tại.");

            store.StoreCode = Trim(store.StoreCode).ToUpperInvariant();
            store.StoreName = Trim(store.StoreName);
            store.Address = Trim(store.Address);
            store.Phone = TrimNullable(store.Phone);
            store.IsActive = true;

            return _storeRepository.Add(store);
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Store store)
        {
            PermissionService.RequireAdmin();
            Validate(store, false);

            if (await _storeRepository.IsStoreCodeExistsAsync(store.StoreCode))
                throw new Exception("Mã cửa hàng đã tồn tại.");
            if (await _storeRepository.IsStoreNameExistsAsync(store.StoreName))
                throw new Exception("Tên cửa hàng đã tồn tại.");

            store.StoreCode = Trim(store.StoreCode).ToUpperInvariant();
            store.StoreName = Trim(store.StoreName);
            store.Address = Trim(store.Address);
            store.Phone = TrimNullable(store.Phone);
            store.IsActive = true;

            return await _storeRepository.AddAsync(store);
        }

        public bool Update(Store store)
        {
            PermissionService.RequireAdmin();
            Require(store.Id > 0, "Id cửa hàng không hợp lệ.");
            Validate(store, true);

            if (_storeRepository.IsStoreCodeExists(store.StoreCode, store.Id))
                throw new Exception("Mã cửa hàng đã tồn tại.");
            if (_storeRepository.IsStoreNameExists(store.StoreName, store.Id))
                throw new Exception("Tên cửa hàng đã tồn tại.");

            store.StoreCode = Trim(store.StoreCode).ToUpperInvariant();
            store.StoreName = Trim(store.StoreName);
            store.Address = Trim(store.Address);
            store.Phone = TrimNullable(store.Phone);

            return _storeRepository.Update(store);
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Store store)
        {
            PermissionService.RequireAdmin();
            Require(store.Id > 0, "Id cửa hàng không hợp lệ.");
            Validate(store, true);

            if (await _storeRepository.IsStoreCodeExistsAsync(store.StoreCode, store.Id))
                throw new Exception("Mã cửa hàng đã tồn tại.");
            if (await _storeRepository.IsStoreNameExistsAsync(store.StoreName, store.Id))
                throw new Exception("Tên cửa hàng đã tồn tại.");

            store.StoreCode = Trim(store.StoreCode).ToUpperInvariant();
            store.StoreName = Trim(store.StoreName);
            store.Address = Trim(store.Address);
            store.Phone = TrimNullable(store.Phone);

            return await _storeRepository.UpdateAsync(store);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id cửa hàng không hợp lệ.");
            return _storeRepository.SetActive(id, isActive);
        }

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id cửa hàng không hợp lệ.");
            return await _storeRepository.SetActiveAsync(id, isActive);
        }

        private void Validate(Store store, bool isUpdate)
        {
            Require(store != null, "Dữ liệu cửa hàng không hợp lệ.");
            Require(!string.IsNullOrWhiteSpace(store.StoreCode), "Mã cửa hàng không được để trống.");
            Require(!string.IsNullOrWhiteSpace(store.StoreName), "Tên cửa hàng không được để trống.");
            Require(!string.IsNullOrWhiteSpace(store.Address), "Địa chỉ cửa hàng không được để trống.");
            Require(store.StoreCode.Length <= 20, "Mã cửa hàng không được vượt quá 20 ký tự.");
            Require(store.StoreName.Length <= 150, "Tên cửa hàng không được vượt quá 150 ký tự.");
        }
    }
}
