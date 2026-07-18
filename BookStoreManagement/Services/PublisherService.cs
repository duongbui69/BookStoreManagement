using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class PublisherService : ServiceBase
    {
        private readonly PublisherRepository _repository;
        public PublisherService() { _repository = new PublisherRepository(); }
        public List<Publisher> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public List<Publisher> GetActive() { PermissionService.RequireStaffOrAdmin(); return _repository.GetActive(); }
        public Publisher? GetById(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id nhà xuất bản không hợp lệ."); return _repository.GetById(id); }
        public List<Publisher> Search(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword); }
        public int Add(Publisher item) { PermissionService.RequireAdmin(); Validate(item); item.PublisherName = Trim(item.PublisherName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); item.IsActive = true; if (_repository.IsNameExists(item.PublisherName)) throw new Exception("Tên nhà xuất bản đã tồn tại."); return _repository.Add(item); }
        public bool Update(Publisher item) { PermissionService.RequireAdmin(); Require(item.Id > 0, "Id nhà xuất bản không hợp lệ."); Validate(item); item.PublisherName = Trim(item.PublisherName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); if (_repository.IsNameExists(item.PublisherName, item.Id)) throw new Exception("Tên nhà xuất bản đã tồn tại."); return _repository.Update(item); }
        public bool SetActive(int id, bool isActive) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà xuất bản không hợp lệ."); return _repository.SetActive(id, isActive); }
        private void Validate(Publisher item) { Require(item != null, "Dữ liệu nhà xuất bản không hợp lệ."); Require(!string.IsNullOrWhiteSpace(item.PublisherName), "Tên nhà xuất bản không được để trống."); Require(item.PublisherName.Length <= 150, "Tên nhà xuất bản không được vượt quá 150 ký tự."); }

        public async System.Threading.Tasks.Task<List<Publisher>> GetAllAsync() { PermissionService.RequireAdmin(); return await _repository.GetAllAsync(); }
        public async System.Threading.Tasks.Task<List<Publisher>> GetActiveAsync() { PermissionService.RequireStaffOrAdmin(); return await _repository.GetActiveAsync(); }
        public async System.Threading.Tasks.Task<Publisher?> GetByIdAsync(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id nhà xuất bản không hợp lệ."); return await _repository.GetByIdAsync(id); }
        public async System.Threading.Tasks.Task<List<Publisher>> SearchAsync(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? await _repository.GetAllAsync() : await _repository.SearchAsync(keyword); }
        public async System.Threading.Tasks.Task<int> AddAsync(Publisher item) { PermissionService.RequireAdmin(); Validate(item); item.PublisherName = Trim(item.PublisherName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); item.IsActive = true; if (await _repository.IsNameExistsAsync(item.PublisherName)) throw new Exception("Tên nhà xuất bản đã tồn tại."); return await _repository.AddAsync(item); }
        public async System.Threading.Tasks.Task<bool> UpdateAsync(Publisher item) { PermissionService.RequireAdmin(); Require(item.Id > 0, "Id nhà xuất bản không hợp lệ."); Validate(item); item.PublisherName = Trim(item.PublisherName); item.Phone = TrimNullable(item.Phone); item.Email = TrimNullable(item.Email); item.Address = TrimNullable(item.Address); if (await _repository.IsNameExistsAsync(item.PublisherName, item.Id)) throw new Exception("Tên nhà xuất bản đã tồn tại."); return await _repository.UpdateAsync(item); }
        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive) { PermissionService.RequireAdmin(); Require(id > 0, "Id nhà xuất bản không hợp lệ."); return await _repository.SetActiveAsync(id, isActive); }
    }
}
