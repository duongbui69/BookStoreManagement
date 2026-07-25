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
        public async System.Threading.Tasks.Task<List<BookListViewModel>> GetAllAsync() { PermissionService.RequireAdmin(); return await _repository.GetAllAsync(); }
        public List<BookListViewModel> GetActive() { PermissionService.RequireStaffOrAdmin(); return _repository.GetActive(); }
        public async System.Threading.Tasks.Task<List<BookListViewModel>> GetActiveAsync() { PermissionService.RequireStaffOrAdmin(); return await _repository.GetActiveAsync(); }
        public Book? GetById(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Invalid book ID."); return _repository.GetById(id); }
        public async System.Threading.Tasks.Task<Book?> GetByIdAsync(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Invalid book ID."); return await _repository.GetByIdAsync(id); }
        public List<BookListViewModel> Search(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetActive() : _repository.Search(keyword); }
        public async System.Threading.Tasks.Task<List<BookListViewModel>> SearchAsync(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? await _repository.GetActiveAsync() : await _repository.SearchAsync(keyword); }
        public List<BookListViewModel> GetByCategoryId(int categoryId) { PermissionService.RequireStaffOrAdmin(); Require(categoryId > 0, "Invalid category ID."); return _repository.GetByCategoryId(categoryId); }
        public async System.Threading.Tasks.Task<List<BookListViewModel>> GetByCategoryIdAsync(int categoryId) { PermissionService.RequireStaffOrAdmin(); Require(categoryId > 0, "Invalid category ID."); return await _repository.GetByCategoryIdAsync(categoryId); }
        public bool HasEnoughStock(int storeId, int bookId, int quantity) { PermissionService.RequireStaffOrAdmin(); storeId = ResolveStoreIdForWrite(storeId); return _repository.HasEnoughStock(storeId, bookId, quantity); }
        public async System.Threading.Tasks.Task<bool> HasEnoughStockAsync(int storeId, int bookId, int quantity) { PermissionService.RequireStaffOrAdmin(); storeId = ResolveStoreIdForWrite(storeId); return await _repository.HasEnoughStockAsync(storeId, bookId, quantity); }

        public int Add(Book book)
        {
            PermissionService.RequireAdmin();
            Validate(book);
            Normalize(book);
            if (_repository.IsBookCodeExists(book.BookCode)) throw new Exception("Book code already exists.");
            if (!string.IsNullOrWhiteSpace(book.ISBN) && _repository.IsISBNExists(book.ISBN)) throw new Exception("ISBN already exists.");
            book.IsActive = true;
            return _repository.Add(book);
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Book book)
        {
            PermissionService.RequireAdmin();
            Validate(book);
            Normalize(book);
            if (await _repository.IsBookCodeExistsAsync(book.BookCode)) throw new Exception("Book code already exists.");
            if (!string.IsNullOrWhiteSpace(book.ISBN) && await _repository.IsISBNExistsAsync(book.ISBN)) throw new Exception("ISBN already exists.");
            book.IsActive = true;
            return await _repository.AddAsync(book);
        }

        public bool Update(Book book)
        {
            PermissionService.RequireAdmin();
            Require(book.Id > 0, "Invalid book ID.");
            Validate(book);
            Normalize(book);
            if (_repository.IsBookCodeExists(book.BookCode, book.Id)) throw new Exception("Book code already exists.");
            if (!string.IsNullOrWhiteSpace(book.ISBN) && _repository.IsISBNExists(book.ISBN, book.Id)) throw new Exception("ISBN already exists.");
            return _repository.Update(book);
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Book book)
        {
            PermissionService.RequireAdmin();
            Require(book.Id > 0, "Invalid book ID.");
            Validate(book);
            Normalize(book);
            if (await _repository.IsBookCodeExistsAsync(book.BookCode, book.Id)) throw new Exception("Book code already exists.");
            if (!string.IsNullOrWhiteSpace(book.ISBN) && await _repository.IsISBNExistsAsync(book.ISBN, book.Id)) throw new Exception("ISBN already exists.");
            return await _repository.UpdateAsync(book);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Invalid book ID.");
            return _repository.SetActive(id, isActive);
        }

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Invalid book ID.");
            return await _repository.SetActiveAsync(id, isActive);
        }

        private void Validate(Book book)
        {
            Require(book != null, "Invalid book data.");
            Require(!string.IsNullOrWhiteSpace(book.BookCode), "Book code cannot be empty.");
            Require(!string.IsNullOrWhiteSpace(book.Title), "Book title cannot be empty.");
            Require(book.CategoryId > 0, "Invalid book category.");
            Require(book.SellingPrice >= 0, "Invalid selling price.");
            Require(book.Quantity >= 0, "Invalid total stock.");
            Require(book.MinStock >= 0, "Invalid min stock.");
            if (book.PublishYear.HasValue) Require(book.PublishYear.Value > 0 && book.PublishYear.Value <= DateTime.Now.Year + 1, "Invalid publication year.");
            if (book.PageCount.HasValue) Require(book.PageCount.Value > 0, "Number of pages must be > 0.");
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
