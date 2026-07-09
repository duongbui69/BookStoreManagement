using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class PublisherService : ServiceBase
    {
        private readonly PublisherRepository _publisherRepository;

        public PublisherService()
        {
            _publisherRepository = new PublisherRepository();
        }

        public List<Publisher> GetAll() => _publisherRepository.GetAll();

        public List<Publisher> GetActive() => _publisherRepository.GetActive();

        public Publisher GetById(int id)
        {
            EnsureId(id, "Id nhà xuất bản");
            return _publisherRepository.GetById(id) ?? throw new Exception("Không tìm thấy nhà xuất bản.");
        }

        public List<Publisher> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _publisherRepository.Search(keyword);
        }

        public int Add(Publisher publisher)
        {
            Validate(publisher);
            NormalizePublisher(publisher);

            if (_publisherRepository.IsNameExists(publisher.PublisherName))
            {
                throw new Exception("Tên nhà xuất bản đã tồn tại.");
            }

            publisher.IsActive = true;
            return _publisherRepository.Add(publisher);
        }

        public bool Update(Publisher publisher)
        {
            EnsureId(publisher.Id, "Id nhà xuất bản");
            Validate(publisher);
            NormalizePublisher(publisher);

            if (_publisherRepository.IsNameExists(publisher.PublisherName, publisher.Id))
            {
                throw new Exception("Tên nhà xuất bản đã tồn tại.");
            }

            return _publisherRepository.Update(publisher);
        }

        public bool SetActive(int id, bool isActive)
        {
            EnsureId(id, "Id nhà xuất bản");
            return _publisherRepository.SetActive(id, isActive);
        }

        private void Validate(Publisher publisher)
        {
            if (publisher == null)
            {
                throw new Exception("Dữ liệu nhà xuất bản không hợp lệ.");
            }

            EnsureRequired(publisher.PublisherName, "Tên nhà xuất bản");
            EnsureMaxLength(publisher.PublisherName, 150, "Tên nhà xuất bản");
            EnsureValidPhone(publisher.Phone);
            EnsureValidEmail(publisher.Email);
            EnsureMaxLength(publisher.Phone, 30, "Số điện thoại");
            EnsureMaxLength(publisher.Email, 100, "Email");
            EnsureMaxLength(publisher.Address, 255, "Địa chỉ");
        }

        private void NormalizePublisher(Publisher publisher)
        {
            publisher.PublisherName = Normalize(publisher.PublisherName);
            publisher.Phone = NormalizeNullable(publisher.Phone);
            publisher.Email = NormalizeNullable(publisher.Email);
            publisher.Address = NormalizeNullable(publisher.Address);
        }
    }
}
