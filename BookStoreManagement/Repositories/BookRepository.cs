using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class BookRepository
    {
        // Execute a SQL query and return the result
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            addParameters?.Invoke(command.Parameters);

            connection.Open();
            return action(command);
        }

        // Map the data from the SqlDataReader to a Book object
        private Book MapBook(SqlDataReader reader)
        {
            return new Book
            {
                Id = DataReaderHelper.GetInt(reader, "Id"),
                BookCode = DataReaderHelper.GetString(reader, "BookCode"),
                ISBN = DataReaderHelper.GetNullableString(reader, "ISBN"),
                Title = DataReaderHelper.GetString(reader, "Title"),
                CategoryId = DataReaderHelper.GetInt(reader, "CategoryId"),
                CategoryName = DataReaderHelper.GetString(reader, "CategoryName"),
                AuthorId = DataReaderHelper.GetNullableInt(reader, "AuthorId"),
                AuthorName = DataReaderHelper.GetNullableString(reader, "AuthorName"),
                PublisherId = DataReaderHelper.GetNullableInt(reader, "PublisherId"),
                PublisherName = DataReaderHelper.GetNullableString(reader, "PublisherName"),
                SellingPrice = DataReaderHelper.GetDecimal(reader, "SellingPrice"),
                Quantity = DataReaderHelper.GetInt(reader, "Quantity"),
                MinStock = DataReaderHelper.GetInt(reader, "MinStock"),
                StockStatus = DataReaderHelper.GetString(reader, "StockStatus"),
                Description = DataReaderHelper.GetNullableString(reader, "Description"),
                ImagePath = DataReaderHelper.GetNullableString(reader, "ImagePath"),
                IsActive = DataReaderHelper.GetBool(reader, "IsActive"),
                CreatedAt = DataReaderHelper.GetDateTime(reader, "CreatedAt"),
                UpdatedAt = DataReaderHelper.GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        // Get all books
        public List<BookListViewModel> GetAll()
        {
            var books = new List<BookListViewModel>();
            const string sql = @"SELECT * FROM vw_BookList ORDER BY Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(MapBookListViewModel(reader));
                }
                return books;
            }, sql);
        }

        // Get all active books
        public List<BookListViewModel> GetActive()
        {
            var books = new List<BookListViewModel>();
            const string sql = @"SELECT * FROM vw_BookList WHERE IsActive = 1 ORDER BY Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(MapBookListViewModel(reader));
                }
                return books;
            }, sql);
        }

        // Get all inactive books
        public List<BookListViewModel> GetInactive()
        {
            var books = new List<BookListViewModel>();
            const string sql = @"SELECT * FROM vw_BookList WHERE IsActive = 0 ORDER BY Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(MapBookListViewModel(reader));
                }
                return books;
            }, sql);
        }

        // Get a book by its ID
        public Book? GetById(int id)
        {
            const string sql = @"
                SELECT 
                    Id,
                    BookCode,
                    ISBN,
                    Title,
                    CategoryId,
                    AuthorId,
                    PublisherId,
                    SellingPrice,
                    Quantity,
                    MinStock,
                    Description,
                    ImagePath,
                    IsActive,
                    CreatedAt,
                    UpdatedAt
                FROM Books
                WHERE Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapBook(reader);
                }
                return null;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", id);
            });)
        }

        public int Add(Book book)
        {
            const string sql = @"
                INSERT INTO Books (BookCode,
                    ISBN,
                    Title,
                    CategoryId,
                    AuthorId,
                    PublisherId,
                    SellingPrice,
                    Quantity,
                    MinStock,
                    Description,
                    ImagePath,
                    IsActive
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @BookCode,
                    @ISBN,
                    @Title,
                    @CategoryId,
                    @AuthorId,
                    @PublisherId,
                    @SellingPrice,
                    @Quantity,
                    @MinStock,
                    @Description,
                    @ImagePath,
                    @IsActive);";
            return ExecuteQuery(command =>
            {
                return (int)command.ExecuteScalar();
            }, sql, parameters =>
            {
                parameters.AddWithValue("@BookCode", book.BookCode);
                parameters.AddWithValue("@ISBN", (object?)book.ISBN ?? DBNull.Value);
                parameters.AddWithValue("@Title", book.Title);
                parameters.AddWithValue("@CategoryId", book.CategoryId);
                parameters.AddWithValue("@AuthorId", (object?)book.AuthorId ?? DBNull.Value);
                parameters.AddWithValue("@PublisherId", (object?)book.PublisherId ?? DBNull.Value);
                parameters.AddWithValue("@SellingPrice", book.SellingPrice);
                parameters.AddWithValue("@Quantity", book.Quantity);
                parameters.AddWithValue("@MinStock", book.MinStock);
                parameters.AddWithValue("@Description", (object?)book.Description ?? DBNull.Value);
                parameters.AddWithValue("@ImagePath", (object?)book.ImagePath ?? DBNull.Value);
                parameters.AddWithValue("@IsActive", book.IsActive);
            });
        }

        public bool Update(Book book)
        {
            const string sql = @"
                UPDATE Books
                SET 
                    BookCode = @BookCode,
                    ISBN = @ISBN,
                    Title = @Title,
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
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", book.Id);
                parameters.AddWithValue("@BookCode", book.BookCode);
                parameters.AddWithValue("@ISBN", (object?)book.ISBN ?? DBNull.Value);
                parameters.AddWithValue("@Title", book.Title);
                parameters.AddWithValue("@CategoryId", book.CategoryId);
                parameters.AddWithValue("@AuthorId", (object?)book.AuthorId ?? DBNull.Value);
                parameters.AddWithValue("@PublisherId", (object?)book.PublisherId ?? DBNull.Value);
                parameters.AddWithValue("@SellingPrice", book.SellingPrice);
                parameters.AddWithValue("@Quantity", book.Quantity);
                parameters.AddWithValue("@MinStock", book.MinStock);
                parameters.AddWithValue("@Description", (object?)book.Description ?? DBNull.Value);
                parameters.AddWithValue("@ImagePath", (object?)book.ImagePath ?? DBNull.Value);
                parameters.AddWithValue("@IsActive", book.IsActive);
            });
        }

        // Set the active status of a book
        public bool SetActive(int bookId, bool isActive)
        {
            const string sql = @"
                UPDATE Books
                SET 
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", bookId);
                parameters.AddWithValue("@IsActive", isActive);
            });
        }

        public bool IsBookCodeExists(string bookCode, int? excludeId = null)
        {
            string sql = "SELECT COUNT(1) FROM Books WHERE BookCode = @BookCode";
            if (excludeId.HasValue)
            {
                sql += " AND Id <> @ExcludeId";
            }



        }


    }
}