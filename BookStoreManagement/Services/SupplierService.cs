using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class SupplierService : ServiceBase
    {
        private readonly SupplierRepository _repository;
        public SupplierService() { _repository = new SupplierRepository(); }
        public List<Supplier> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public async System.Threading.Tasks.Task<List<Supplier>> GetAllAsync() { PermissionService.RequireAdmin(); return await _repository.GetAllAsync(); }
        
        public List<Supplier> GetActive() { PermissionService.RequireAdmin(); return _repository.GetActive(); }
        public async System.Threading.Tasks.Task<List<Supplier>> GetActiveAsync() { PermissionService.RequireAdmin(); return await _repository.GetActiveAsync(); }
        
        public Supplier? GetById(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà cung cấp không hợp lệ."); return _repository.GetById(id); }
        public async System.Threading.Tasks.Task<Supplier?> GetByIdAsync(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà cung cấp không hợp lệ."); return await _repository.GetByIdAsync(id); }
        
        public List<Supplier> Search(string keyword) { PermissionService.RequireAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword); }
        public async System.Threading.Tasks.Task<List<Supplier>> SearchAsync(string keyword) { PermissionService.RequireAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? await _repository.GetAllAsync() : await _repository.SearchAsync(keyword); }
        
        public int Add(Supplier item) { PermissionService.RequireAdmin(); Validate(item); item.SupplierName = Trim(item.SupplierName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); item.IsActive = true; if (_repository.IsNameExists(item.SupplierName)) throw new Exception("Tên nhà cung cấp đã tồn tại."); return _repository.Add(item); }
        public async System.Threading.Tasks.Task<int> AddAsync(Supplier item) { PermissionService.RequireAdmin(); Validate(item); item.SupplierName = Trim(item.SupplierName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); item.IsActive = true; if (await _repository.IsNameExistsAsync(item.SupplierName)) throw new Exception("Tên nhà cung cấp đã tồn tại."); return await _repository.AddAsync(item); }
        
        public bool Update(Supplier item) { PermissionService.RequireAdmin(); Require(item.Id > 0, "Id nhà cung cấp không hợp lệ."); Validate(item); item.SupplierName = Trim(item.SupplierName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); if (_repository.IsNameExists(item.SupplierName, item.Id)) throw new Exception("Tên nhà cung cấp đã tồn tại."); return _repository.Update(item); }
        public async System.Threading.Tasks.Task<bool> UpdateAsync(Supplier item) { PermissionService.RequireAdmin(); Require(item.Id > 0, "Id nhà cung cấp không hợp lệ."); Validate(item); item.SupplierName = Trim(item.SupplierName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); if (await _repository.IsNameExistsAsync(item.SupplierName, item.Id)) throw new Exception("Tên nhà cung cấp đã tồn tại."); return await _repository.UpdateAsync(item); }
        
        public bool SetActive(int id, bool isActive) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà cung cấp không hợp lệ."); return _repository.SetActive(id, isActive); }
        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà cung cấp không hợp lệ."); return await _repository.SetActiveAsync(id, isActive); }
        private void Validate(Supplier item) { Require(item != null, "Dữ liệu nhà cung cấp không hợp lệ."); Require(!string.IsNullOrWhiteSpace(item.SupplierName), "Tên nhà cung cấp không được để trống."); Require(item.SupplierName.Length <= 150, "Tên nhà cung cấp không được vượt quá 150 ký tự."); }
    }
}
