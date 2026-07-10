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
        public List<Supplier> GetActive() { PermissionService.RequireAdmin(); return _repository.GetActive(); }
        public Supplier? GetById(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà cung cấp không hợp lệ."); return _repository.GetById(id); }
        public List<Supplier> Search(string keyword) { PermissionService.RequireAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword); }
        public int Add(Supplier item) { PermissionService.RequireAdmin(); Validate(item); item.SupplierName = Trim(item.SupplierName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); item.IsActive = true; if (_repository.IsNameExists(item.SupplierName)) throw new Exception("Tên nhà cung cấp đã tồn tại."); return _repository.Add(item); }
        public bool Update(Supplier item) { PermissionService.RequireAdmin(); Require(item.Id > 0, "Id nhà cung cấp không hợp lệ."); Validate(item); item.SupplierName = Trim(item.SupplierName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); if (_repository.IsNameExists(item.SupplierName, item.Id)) throw new Exception("Tên nhà cung cấp đã tồn tại."); return _repository.Update(item); }
        public bool SetActive(int id, bool isActive) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà cung cấp không hợp lệ."); return _repository.SetActive(id, isActive); }
        private void Validate(Supplier item) { Require(item != null, "Dữ liệu nhà cung cấp không hợp lệ."); Require(!string.IsNullOrWhiteSpace(item.SupplierName), "Tên nhà cung cấp không được để trống."); Require(item.SupplierName.Length <= 150, "Tên nhà cung cấp không được vượt quá 150 ký tự."); }
    }
}
