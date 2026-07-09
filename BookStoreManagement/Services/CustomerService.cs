using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class CustomerService : ServiceBase
    {
        private readonly CustomerRepository _customerRepository;

        public CustomerService()
        {
            _customerRepository = new CustomerRepository();
        }

        public List<Customer> GetAll() => _customerRepository.GetAll();

        public Customer GetById(int id)
        {
            EnsureId(id, "Id khách hàng");
            return _customerRepository.GetById(id) ?? throw new Exception("Không tìm thấy khách hàng.");
        }

        public Customer? GetRetailCustomer()
        {
            return _customerRepository.GetRetailCustomer();
        }

        public List<Customer> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _customerRepository.Search(keyword);
        }

        public int Add(Customer customer)
        {
            Validate(customer);
            NormalizeCustomer(customer);
            return _customerRepository.Add(customer);
        }

        public bool Update(Customer customer)
        {
            EnsureId(customer.Id, "Id khách hàng");
            Validate(customer);
            NormalizeCustomer(customer);
            return _customerRepository.Update(customer);
        }

        public bool Delete(int id)
        {
            EnsureId(id, "Id khách hàng");
            return _customerRepository.Delete(id);
        }

        private void Validate(Customer customer)
        {
            if (customer == null)
            {
                throw new Exception("Dữ liệu khách hàng không hợp lệ.");
            }

            EnsureRequired(customer.FullName, "Tên khách hàng");
            EnsureMaxLength(customer.FullName, 150, "Tên khách hàng");
            EnsureValidPhone(customer.Phone);
            EnsureValidEmail(customer.Email);
            EnsureMaxLength(customer.Phone, 30, "Số điện thoại");
            EnsureMaxLength(customer.Email, 100, "Email");
            EnsureMaxLength(customer.Address, 255, "Địa chỉ");
        }

        private void NormalizeCustomer(Customer customer)
        {
            customer.FullName = Normalize(customer.FullName);
            customer.Phone = NormalizeNullable(customer.Phone);
            customer.Email = NormalizeNullable(customer.Email);
            customer.Address = NormalizeNullable(customer.Address);
        }
    }
}
