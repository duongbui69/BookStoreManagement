using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class CategoryService : ServiceBase
    {
        private readonly CategoryRepository _repository;
        public CategoryService() { _repository = new CategoryRepository(); }

        public (List<CategoryItem> Items, int TotalCount) GetPagedCategories(int page, int pageSize, string searchTerm = "", string statusFilter = "")
        {
            PermissionService.RequireStaffOrAdmin();
            searchTerm = Trim(searchTerm);
            return _repository.GetPagedCategories(page, pageSize, searchTerm, statusFilter);
        }

        public async System.Threading.Tasks.Task<(List<CategoryItem> Items, int TotalCount)> GetPagedCategoriesAsync(int page, int pageSize, string searchTerm = "", string statusFilter = "")
        {
            PermissionService.RequireStaffOrAdmin();
            searchTerm = Trim(searchTerm);
            return await _repository.GetPagedCategoriesAsync(page, pageSize, searchTerm, statusFilter);
        }

        public List<Category> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public async System.Threading.Tasks.Task<List<Category>> GetAllAsync() { PermissionService.RequireAdmin(); return await _repository.GetAllAsync(); }
        
        public List<Category> GetActive() { PermissionService.RequireStaffOrAdmin(); return _repository.GetActive(); }
        public async System.Threading.Tasks.Task<List<Category>> GetActiveAsync() { PermissionService.RequireStaffOrAdmin(); return await _repository.GetActiveAsync(); }
        
        public Category? GetById(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id danh mục không hợp lệ."); return _repository.GetById(id); }
        public async System.Threading.Tasks.Task<Category?> GetByIdAsync(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id danh mục không hợp lệ."); return await _repository.GetByIdAsync(id); }
        
        public List<Category> Search(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword); }
        public async System.Threading.Tasks.Task<List<Category>> SearchAsync(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? await _repository.GetAllAsync() : await _repository.SearchAsync(keyword); }

        public int Add(Category category)
        {
            PermissionService.RequireAdmin();
            Validate(category);
            category.CategoryName = Trim(category.CategoryName);
            category.Description = TrimNullable(category.Description);
            category.IsActive = true;
            if (_repository.IsNameExists(category.CategoryName)) throw new Exception("Tên danh mục đã tồn tại.");
            return _repository.Add(category);
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Category category)
        {
            PermissionService.RequireAdmin();
            Validate(category);
            category.CategoryName = Trim(category.CategoryName);
            category.Description = TrimNullable(category.Description);
            category.IsActive = true;
            if (await _repository.IsNameExistsAsync(category.CategoryName)) throw new Exception("Tên danh mục đã tồn tại.");
            return await _repository.AddAsync(category);
        }

        public bool Update(Category category)
        {
            PermissionService.RequireAdmin();
            Require(category.Id > 0, "Id danh mục không hợp lệ.");
            Validate(category);
            category.CategoryName = Trim(category.CategoryName);
            category.Description = TrimNullable(category.Description);
            if (_repository.IsNameExists(category.CategoryName, category.Id)) throw new Exception("Tên danh mục đã tồn tại.");
            return _repository.Update(category);
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Category category)
        {
            PermissionService.RequireAdmin();
            Require(category.Id > 0, "Id danh mục không hợp lệ.");
            Validate(category);
            category.CategoryName = Trim(category.CategoryName);
            category.Description = TrimNullable(category.Description);
            if (await _repository.IsNameExistsAsync(category.CategoryName, category.Id)) throw new Exception("Tên danh mục đã tồn tại.");
            return await _repository.UpdateAsync(category);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id danh mục không hợp lệ.");
            return _repository.SetActive(id, isActive);
        }

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id danh mục không hợp lệ.");
            return await _repository.SetActiveAsync(id, isActive);
        }

        private void Validate(Category category)
        {
            Require(category != null, "Dữ liệu danh mục không hợp lệ.");
            Require(!string.IsNullOrWhiteSpace(category.CategoryName), "Tên danh mục không được để trống.");
            Require(category.CategoryName.Length <= 100, "Tên danh mục không được vượt quá 100 ký tự.");
        }
    }
}
