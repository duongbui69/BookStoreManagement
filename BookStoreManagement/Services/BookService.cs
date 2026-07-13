using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class BookService : ServiceBase
    {
        private readonly BookRepository _repository;
        public BookService() { _repository = new BookRepository(); }

        public List<BookListViewModel> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public List<BookListViewModel> GetActive() { PermissionService.RequireStaffOrAdmin(); return _repository.GetActive(); }
        public Book? GetById(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id sách không hợp lệ."); return _repository.GetById(id); }
        public List<BookListViewModel> Search(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetActive() : _repository.Search(keyword); }
        public List<BookListViewModel> GetByCategoryId(int categoryId) { PermissionService.RequireStaffOrAdmin(); Require(categoryId > 0, "Id danh mục không hợp lệ."); return _repository.GetByCategoryId(categoryId); }
        public bool HasEnoughStock(int storeId, int bookId, int quantity) { PermissionService.RequireStaffOrAdmin(); storeId = ResolveStoreIdForWrite(storeId); return _repository.HasEnoughStock(storeId, bookId, quantity); }

        public int Add(Book book)
        {
            PermissionService.RequireAdmin();
            Validate(book);
            Normalize(book);
            if (_repository.IsBookCodeExists(book.BookCode)) throw new Exception("Mã sách đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(book.ISBN) && _repository.IsISBNExists(book.ISBN)) throw new Exception("ISBN đã tồn tại.");
            book.IsActive = true;
            return _repository.Add(book);
        }

        public bool Update(Book book)
        {
            PermissionService.RequireAdmin();
            Require(book.Id > 0, "Id sách không hợp lệ.");
            Validate(book);
            Normalize(book);
            if (_repository.IsBookCodeExists(book.BookCode, book.Id)) throw new Exception("Mã sách đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(book.ISBN) && _repository.IsISBNExists(book.ISBN, book.Id)) throw new Exception("ISBN đã tồn tại.");
            return _repository.Update(book);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id sách không hợp lệ.");
            return _repository.SetActive(id, isActive);
        }

        private void Validate(Book book)
        {
            Require(book != null, "Dữ liệu sách không hợp lệ.");
            Require(!string.IsNullOrWhiteSpace(book.BookCode), "Mã sách không được để trống.");
            Require(!string.IsNullOrWhiteSpace(book.Title), "Tên sách không được để trống.");
            Require(book.CategoryId > 0, "Danh mục sách không hợp lệ.");
            Require(book.SellingPrice >= 0, "Giá bán không hợp lệ.");
            Require(book.Quantity >= 0, "Tổng tồn kho không hợp lệ.");
            Require(book.MinStock >= 0, "Tồn tối thiểu không hợp lệ.");
            if (book.PublishYear.HasValue) Require(book.PublishYear.Value > 0 && book.PublishYear.Value <= DateTime.Now.Year + 1, "Năm xuất bản không hợp lệ.");
            if (book.PageCount.HasValue) Require(book.PageCount.Value > 0, "Số trang phải lớn hơn 0.");
        }

        private void Normalize(Book book)
        {
            book.BookCode = Trim(book.BookCode).ToUpperInvariant();
            book.ISBN = TrimNullable(book.ISBN);
            book.Title = Trim(book.Title);
            book.Description = TrimNullable(book.Description);
            book.ImagePath = TrimNullable(book.ImagePath);
        }
    }
}
