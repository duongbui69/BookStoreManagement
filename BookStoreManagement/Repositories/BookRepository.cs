using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class BookRepository : RepositoryBase
    {
        private Book MapBook(SqlDataReader reader)
        {
            return new Book
            {
                Id = GetInt(reader, "Id"),
                BookCode = GetString(reader, "BookCode"),
                ISBN = GetNullableString(reader, "ISBN"),
                Title = GetString(reader, "Title"),
                PublishYear = GetNullableInt(reader, "PublishYear"),
                PageCount = GetNullableInt(reader, "PageCount"),
                CategoryId = GetInt(reader, "CategoryId"),
                AuthorId = GetNullableInt(reader, "AuthorId"),
                PublisherId = GetNullableInt(reader, "PublisherId"),
                SellingPrice = GetDecimal(reader, "SellingPrice"),
                Quantity = GetInt(reader, "Quantity"),
                MinStock = GetInt(reader, "MinStock"),
                Description = GetNullableString(reader, "Description"),
                ImagePath = GetNullableString(reader, "ImagePath"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        private BookListViewModel MapBookList(SqlDataReader reader)
        {
            return new BookListViewModel
            {
                Id = GetInt(reader, "Id"),
                BookCode = GetString(reader, "BookCode"),
                ISBN = GetNullableString(reader, "ISBN"),
                Title = GetString(reader, "Title"),
                PublishYear = GetNullableInt(reader, "PublishYear"),
                PageCount = GetNullableInt(reader, "PageCount"),
                CategoryId = GetInt(reader, "CategoryId"),
                CategoryName = GetString(reader, "CategoryName"),
                AuthorId = GetNullableInt(reader, "AuthorId"),
                AuthorName = GetNullableString(reader, "AuthorName"),
                PublisherId = GetNullableInt(reader, "PublisherId"),
                PublisherName = GetNullableString(reader, "PublisherName"),
                SellingPrice = GetDecimal(reader, "SellingPrice"),
                Quantity = GetInt(reader, "Quantity"),
                MinStock = GetInt(reader, "MinStock"),
                StockStatus = GetString(reader, "StockStatus"),
                Description = GetNullableString(reader, "Description"),
                ImagePath = GetNullableString(reader, "ImagePath"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<BookListViewModel> GetAll()
        {
            var books = new List<BookListViewModel>();
            const string sql = "SELECT * FROM vw_BookList ORDER BY Id DESC;";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) books.Add(MapBookList(reader));
                return books;
            }, sql);
        }

        public async System.Threading.Tasks.Task<List<BookListViewModel>> GetAllAsync()
        {
            const string sql = "SELECT * FROM vw_BookList ORDER BY Id DESC;";
            var result = await QueryAsync<BookListViewModel>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public List<BookListViewModel> GetActive()
        {
            var books = new List<BookListViewModel>();
            const string sql = "SELECT * FROM vw_BookList WHERE IsActive = 1 ORDER BY Title;";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) books.Add(MapBookList(reader));
                return books;
            }, sql);
        }

        public async System.Threading.Tasks.Task<List<BookListViewModel>> GetActiveAsync()
        {
            const string sql = "SELECT * FROM vw_BookList WHERE IsActive = 1 ORDER BY Title;";
            var result = await QueryAsync<BookListViewModel>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public Book? GetById(int id)
        {
            const string sql = @"
                SELECT Id, BookCode, ISBN, Title, PublishYear, PageCount, CategoryId, AuthorId, PublisherId,
                       SellingPrice, Quantity, MinStock, Description, ImagePath, IsActive, CreatedAt, UpdatedAt
                FROM Books
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapBook(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async System.Threading.Tasks.Task<Book?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, BookCode, ISBN, Title, PublishYear, PageCount, CategoryId, AuthorId, PublisherId,
                       SellingPrice, Quantity, MinStock, Description, ImagePath, IsActive, CreatedAt, UpdatedAt
                FROM Books
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Book>(sql, new { Id = id });
        }

        public List<BookListViewModel> Search(string keyword)
        {
            var books = new List<BookListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_BookList
                WHERE BookCode LIKE N'%' + @Keyword + N'%'
                   OR ISBN LIKE N'%' + @Keyword + N'%'
                   OR Title LIKE N'%' + @Keyword + N'%'
                   OR CategoryName LIKE N'%' + @Keyword + N'%'
                   OR AuthorName LIKE N'%' + @Keyword + N'%'
                   OR PublisherName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) books.Add(MapBookList(reader));
                return books;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public async System.Threading.Tasks.Task<List<BookListViewModel>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT *
                FROM vw_BookList
                WHERE BookCode LIKE N'%' + @Keyword + N'%'
                   OR ISBN LIKE N'%' + @Keyword + N'%'
                   OR Title LIKE N'%' + @Keyword + N'%'
                   OR CategoryName LIKE N'%' + @Keyword + N'%'
                   OR AuthorName LIKE N'%' + @Keyword + N'%'
                   OR PublisherName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<BookListViewModel>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public List<BookListViewModel> GetByCategoryId(int categoryId)
        {
            var books = new List<BookListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_BookList
                WHERE CategoryId = @CategoryId
                ORDER BY Title;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) books.Add(MapBookList(reader));
                return books;
            }, sql, parameters => AddParameter(parameters, "@CategoryId", categoryId));
        }

        public async System.Threading.Tasks.Task<List<BookListViewModel>> GetByCategoryIdAsync(int categoryId)
        {
            const string sql = @"
                SELECT *
                FROM vw_BookList
                WHERE CategoryId = @CategoryId
                ORDER BY Title;
            ";
            var result = await QueryAsync<BookListViewModel>(sql, new { CategoryId = categoryId });
            return System.Linq.Enumerable.ToList(result);
        }

        public int Add(Book book)
        {
            const string sql = @"
                INSERT INTO Books (
                    BookCode, ISBN, Title, PublishYear, PageCount, CategoryId, AuthorId, PublisherId,
                    SellingPrice, Quantity, MinStock, Description, ImagePath, IsActive
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @BookCode, @ISBN, @Title, @PublishYear, @PageCount, @CategoryId, @AuthorId, @PublisherId,
                    @SellingPrice, @Quantity, @MinStock, @Description, @ImagePath, @IsActive
                );
            ";
            return ExecuteScalarInt(sql, parameters => AddBookParameters(parameters, book, includeId: false));
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Book book)
        {
            const string sql = @"
                INSERT INTO Books (
                    BookCode, ISBN, Title, PublishYear, PageCount, CategoryId, AuthorId, PublisherId,
                    SellingPrice, Quantity, MinStock, Description, ImagePath, IsActive
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @BookCode, @ISBN, @Title, @PublishYear, @PageCount, @CategoryId, @AuthorId, @PublisherId,
                    @SellingPrice, @Quantity, @MinStock, @Description, @ImagePath, @IsActive
                );
            ";
            return await ExecuteScalarAsync<int>(sql, book);
        }

        public bool Update(Book book)
        {
            const string sql = @"
                UPDATE Books
                SET BookCode = @BookCode,
                    ISBN = @ISBN,
                    Title = @Title,
                    PublishYear = @PublishYear,
                    PageCount = @PageCount,
                    CategoryId = @CategoryId,
                    AuthorId = @AuthorId,
                    PublisherId = @PublisherId,
                    SellingPrice = @SellingPrice,
                    Quantity = @Quantity,
                    MinStock = @MinStock,
                    Description = @Description,
                    ImagePath = @ImagePath,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters => AddBookParameters(parameters, book, includeId: true)) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Book book)
        {
            const string sql = @"
                UPDATE Books
                SET BookCode = @BookCode,
                    ISBN = @ISBN,
                    Title = @Title,
                    PublishYear = @PublishYear,
                    PageCount = @PageCount,
                    CategoryId = @CategoryId,
                    AuthorId = @AuthorId,
                    PublisherId = @PublisherId,
                    SellingPrice = @SellingPrice,
                    Quantity = @Quantity,
                    MinStock = @MinStock,
                    Description = @Description,
                    ImagePath = @ImagePath,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, book) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Books
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", id);
                AddParameter(parameters, "@IsActive", isActive);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Books
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new { Id = id, IsActive = isActive }) > 0;
        }

        public bool HasEnoughStock(int storeId, int bookId, int quantity)
        {
            // Ignore storeId for single store DB
            const string sql = @"
                SELECT COUNT(1)
                FROM Books
                WHERE Id = @BookId
                  AND Quantity >= @Quantity
                  AND IsActive = 1;
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@BookId", bookId);
                AddParameter(parameters, "@Quantity", quantity);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> HasEnoughStockAsync(int storeId, int bookId, int quantity)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Books
                WHERE Id = @BookId
                  AND Quantity >= @Quantity
                  AND IsActive = 1;
            ";
            return await ExecuteScalarAsync<int>(sql, new { BookId = bookId, Quantity = quantity }) > 0;
        }

        public bool IsBookCodeExists(string bookCode, int? excludeId = null)
        {
            string sql = "SELECT COUNT(1) FROM Books WHERE BookCode = @BookCode";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@BookCode", bookCode);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> IsBookCodeExistsAsync(string bookCode, int? excludeId = null)
        {
            string sql = "SELECT COUNT(1) FROM Books WHERE BookCode = @BookCode";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return await ExecuteScalarAsync<int>(sql, new { BookCode = bookCode, ExcludeId = excludeId }) > 0;
        }

        public bool IsISBNExists(string isbn, int? excludeId = null)
        {
            string sql = "SELECT COUNT(1) FROM Books WHERE ISBN = @ISBN";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@ISBN", isbn);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> IsISBNExistsAsync(string isbn, int? excludeId = null)
        {
            string sql = "SELECT COUNT(1) FROM Books WHERE ISBN = @ISBN";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return await ExecuteScalarAsync<int>(sql, new { ISBN = isbn, ExcludeId = excludeId }) > 0;
        }

        private void AddBookParameters(SqlParameterCollection parameters, Book book, bool includeId)
        {
            if (includeId) AddParameter(parameters, "@Id", book.Id);
            AddParameter(parameters, "@BookCode", book.BookCode);
            AddParameter(parameters, "@ISBN", book.ISBN);
            AddParameter(parameters, "@Title", book.Title);
            AddParameter(parameters, "@PublishYear", book.PublishYear);
            AddParameter(parameters, "@PageCount", book.PageCount);
            AddParameter(parameters, "@CategoryId", book.CategoryId);
            AddParameter(parameters, "@AuthorId", book.AuthorId);
            AddParameter(parameters, "@PublisherId", book.PublisherId);
            AddParameter(parameters, "@SellingPrice", book.SellingPrice);
            AddParameter(parameters, "@Quantity", book.Quantity);
            AddParameter(parameters, "@MinStock", book.MinStock);
            AddParameter(parameters, "@Description", book.Description);
            AddParameter(parameters, "@ImagePath", book.ImagePath);
            AddParameter(parameters, "@IsActive", book.IsActive);
        }
    }
}
