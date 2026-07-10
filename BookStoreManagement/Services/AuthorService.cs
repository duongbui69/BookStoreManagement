using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class AuthorService : ServiceBase
    {
        private readonly AuthorRepository _repository;
        public AuthorService() { _repository = new AuthorRepository(); }
        public List<Author> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public List<Author> GetActive() { PermissionService.RequireStaffOrAdmin(); return _repository.GetActive(); }
        public Author? GetById(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id tác giả không hợp lệ."); return _repository.GetById(id); }
        public List<Author> Search(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword); }
        public int Add(Author item) { PermissionService.RequireAdmin(); Validate(item); item.AuthorName = Trim(item.AuthorName); item.Description = TrimNullable(item.Description); item.IsActive = true; if (_repository.IsNameExists(item.AuthorName)) throw new Exception("Tên tác giả đã tồn tại."); return _repository.Add(item); }
        public bool Update(Author item) { PermissionService.RequireAdmin(); Require(item.Id > 0, "Id tác giả không hợp lệ."); Validate(item); item.AuthorName = Trim(item.AuthorName); item.Description = TrimNullable(item.Description); if (_repository.IsNameExists(item.AuthorName, item.Id)) throw new Exception("Tên tác giả đã tồn tại."); return _repository.Update(item); }
        public bool SetActive(int id, bool isActive) { PermissionService.RequireAdmin(); Require(id > 0, "Id tác giả không hợp lệ."); return _repository.SetActive(id, isActive); }
        private void Validate(Author item) { Require(item != null, "Dữ liệu tác giả không hợp lệ."); Require(!string.IsNullOrWhiteSpace(item.AuthorName), "Tên tác giả không được để trống."); Require(item.AuthorName.Length <= 150, "Tên tác giả không được vượt quá 150 ký tự."); }
    }
}
