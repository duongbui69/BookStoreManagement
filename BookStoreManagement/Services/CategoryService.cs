using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class CategoryService : ServiceBase
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService()
        {
            _categoryRepository = new CategoryRepository();
        }

        public List<Category> GetAll() => _categoryRepository.GetAll();

        public List<Category> GetActive() => _categoryRepository.GetActive();

        public Category GetById(int id)
        {
            EnsureId(id, "Id danh mục");
            return _categoryRepository.GetById(id) ?? throw new Exception("Không tìm thấy danh mục.");
        }

        public List<Category> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _categoryRepository.Search(keyword);
        }

        public int Add(Category category)
        {
            Validate(category);

            category.CategoryName = Normalize(category.CategoryName);
            category.Description = NormalizeNullable(category.Description);
            category.IsActive = true;

            if (_categoryRepository.IsNameExists(category.CategoryName))
            {
                throw new Exception("Tên danh mục đã tồn tại.");
            }

            return _categoryRepository.Add(category);
        }

        public bool Update(Category category)
        {
            EnsureId(category.Id, "Id danh mục");
            Validate(category);

            category.CategoryName = Normalize(category.CategoryName);
            category.Description = NormalizeNullable(category.Description);

            if (_categoryRepository.IsNameExists(category.CategoryName, category.Id))
            {
                throw new Exception("Tên danh mục đã tồn tại.");
            }

            return _categoryRepository.Update(category);
        }

        public bool SetActive(int id, bool isActive)
        {
            EnsureId(id, "Id danh mục");
            return _categoryRepository.SetActive(id, isActive);
        }

        private void Validate(Category category)
        {
            if (category == null)
            {
                throw new Exception("Dữ liệu danh mục không hợp lệ.");
            }

            EnsureRequired(category.CategoryName, "Tên danh mục");
            EnsureMaxLength(category.CategoryName, 100, "Tên danh mục");
            EnsureMaxLength(category.Description, 500, "Mô tả");
        }
    }
}
