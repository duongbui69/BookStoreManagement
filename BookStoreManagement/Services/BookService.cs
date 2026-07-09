using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class BookService : ServiceBase
    {
        private readonly BookRepository _bookRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly AuthorRepository _authorRepository;
        private readonly PublisherRepository _publisherRepository;
        private readonly FileStorageService _fileStorageService;

        public BookService()
        {
            _bookRepository = new BookRepository();
            _categoryRepository = new CategoryRepository();
            _authorRepository = new AuthorRepository();
            _publisherRepository = new PublisherRepository();
            _fileStorageService = new FileStorageService();
        }

        public List<BookListViewModel> GetAll() => _bookRepository.GetAll();

        public List<BookListViewModel> GetActive() => _bookRepository.GetActive();

        public Book GetById(int id)
        {
            EnsureId(id, "Id sách");
            return _bookRepository.GetById(id) ?? throw new Exception("Không tìm thấy sách.");
        }

        public BookListViewModel GetViewById(int id)
        {
            EnsureId(id, "Id sách");
            return _bookRepository.GetViewById(id) ?? throw new Exception("Không tìm thấy sách.");
        }

        public List<BookListViewModel> Search(string keyword, bool activeOnly = false)
        {
            keyword = Normalize(keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return activeOnly ? GetActive() : GetAll();
            }

            return activeOnly ? _bookRepository.SearchActive(keyword) : _bookRepository.Search(keyword);
        }

        public int Add(Book book, string? sourceImageFilePath = null)
        {
            ValidateBook(book, isCreate: true);
            NormalizeBook(book);

            if (_bookRepository.IsBookCodeExists(book.BookCode))
            {
                throw new Exception("Mã sách đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(book.ISBN) && _bookRepository.IsISBNExists(book.ISBN))
            {
                throw new Exception("ISBN đã tồn tại.");
            }

            ValidateForeignKeys(book);

            if (!string.IsNullOrWhiteSpace(sourceImageFilePath))
            {
                book.ImagePath = _fileStorageService.SaveBookImage(sourceImageFilePath);
            }

            book.IsActive = true;
            return _bookRepository.Add(book);
        }

        public bool Update(Book book, string? newSourceImageFilePath = null)
        {
            EnsureId(book.Id, "Id sách");
            ValidateBook(book, isCreate: false);
            NormalizeBook(book);

            Book oldBook = GetById(book.Id);

            if (_bookRepository.IsBookCodeExists(book.BookCode, book.Id))
            {
                throw new Exception("Mã sách đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(book.ISBN) && _bookRepository.IsISBNExists(book.ISBN, book.Id))
            {
                throw new Exception("ISBN đã tồn tại.");
            }

            ValidateForeignKeys(book);

            if (!string.IsNullOrWhiteSpace(newSourceImageFilePath))
            {
                book.ImagePath = _fileStorageService.SaveBookImage(newSourceImageFilePath);
                _fileStorageService.DeleteFileIfExists(oldBook.ImagePath);
            }
            else
            {
                book.ImagePath = oldBook.ImagePath;
            }

            return _bookRepository.Update(book);
        }

        public bool UpdateImage(int bookId, string sourceImageFilePath)
        {
            EnsureId(bookId, "Id sách");
            Book oldBook = GetById(bookId);

            string? newImagePath = _fileStorageService.SaveBookImage(sourceImageFilePath);
            bool result = _bookRepository.UpdateImagePath(bookId, newImagePath);

            if (result)
            {
                _fileStorageService.DeleteFileIfExists(oldBook.ImagePath);
            }

            return result;
        }

        public bool SetActive(int id, bool isActive)
        {
            EnsureId(id, "Id sách");
            return _bookRepository.SetActive(id, isActive);
        }

        public bool HasEnoughStock(int bookId, int quantity)
        {
            EnsureId(bookId, "Id sách");
            EnsurePositive(quantity, "Số lượng");
            return _bookRepository.HasEnoughStock(bookId, quantity);
        }

        public int GetCurrentQuantity(int bookId)
        {
            EnsureId(bookId, "Id sách");
            return _bookRepository.GetCurrentQuantity(bookId);
        }

        private void ValidateBook(Book book, bool isCreate)
        {
            if (book == null)
            {
                throw new Exception("Dữ liệu sách không hợp lệ.");
            }

            EnsureRequired(book.BookCode, "Mã sách");
            EnsureRequired(book.Title, "Tên sách");
            EnsureId(book.CategoryId, "Danh mục");
            EnsurePositive(book.SellingPrice, "Giá bán");
            EnsureNonNegative(book.Quantity, "Số lượng tồn");
            EnsureNonNegative(book.MinStock, "Tồn kho tối thiểu");
            EnsureMaxLength(book.BookCode, 50, "Mã sách");
            EnsureMaxLength(book.ISBN, 30, "ISBN");
            EnsureMaxLength(book.Title, 255, "Tên sách");
            EnsureMaxLength(book.Description, 1000, "Mô tả");
            EnsureMaxLength(book.ImagePath, 255, "Đường dẫn ảnh");
        }

        private void NormalizeBook(Book book)
        {
            book.BookCode = Normalize(book.BookCode);
            book.ISBN = NormalizeNullable(book.ISBN);
            book.Title = Normalize(book.Title);
            book.Description = NormalizeNullable(book.Description);
            book.ImagePath = NormalizeNullable(book.ImagePath);
        }

        private void ValidateForeignKeys(Book book)
        {
            if (_categoryRepository.GetById(book.CategoryId) == null)
            {
                throw new Exception("Danh mục không tồn tại.");
            }

            if (book.AuthorId.HasValue && _authorRepository.GetById(book.AuthorId.Value) == null)
            {
                throw new Exception("Tác giả không tồn tại.");
            }

            if (book.PublisherId.HasValue && _publisherRepository.GetById(book.PublisherId.Value) == null)
            {
                throw new Exception("Nhà xuất bản không tồn tại.");
            }
        }
    }
}
