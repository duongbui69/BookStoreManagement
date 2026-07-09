using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class AuthorService : ServiceBase
    {
        private readonly AuthorRepository _authorRepository;

        public AuthorService()
        {
            _authorRepository = new AuthorRepository();
        }

        public List<Author> GetAll() => _authorRepository.GetAll();

        public List<Author> GetActive() => _authorRepository.GetActive();

        public Author GetById(int id)
        {
            EnsureId(id, "Id tác giả");
            return _authorRepository.GetById(id) ?? throw new Exception("Không tìm thấy tác giả.");
        }

        public List<Author> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _authorRepository.Search(keyword);
        }

        public int Add(Author author)
        {
            Validate(author);

            author.AuthorName = Normalize(author.AuthorName);
            author.Description = NormalizeNullable(author.Description);
            author.IsActive = true;

            if (_authorRepository.IsNameExists(author.AuthorName))
            {
                throw new Exception("Tên tác giả đã tồn tại.");
            }

            return _authorRepository.Add(author);
        }

        public bool Update(Author author)
        {
            EnsureId(author.Id, "Id tác giả");
            Validate(author);

            author.AuthorName = Normalize(author.AuthorName);
            author.Description = NormalizeNullable(author.Description);

            if (_authorRepository.IsNameExists(author.AuthorName, author.Id))
            {
                throw new Exception("Tên tác giả đã tồn tại.");
            }

            return _authorRepository.Update(author);
        }

        public bool SetActive(int id, bool isActive)
        {
            EnsureId(id, "Id tác giả");
            return _authorRepository.SetActive(id, isActive);
        }

        private void Validate(Author author)
        {
            if (author == null)
            {
                throw new Exception("Dữ liệu tác giả không hợp lệ.");
            }

            EnsureRequired(author.AuthorName, "Tên tác giả");
            EnsureMaxLength(author.AuthorName, 150, "Tên tác giả");
            EnsureMaxLength(author.Description, 500, "Mô tả");
        }
    }
}
