using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class CustomerService : ServiceBase
    {
        private readonly CustomerRepository _repository;
        public CustomerService() { _repository = new CustomerRepository(); }

        public List<Customer> GetAll() { PermissionService.RequireStaffOrAdmin(); return _repository.GetAll(); }
        public List<Customer> GetActive() { PermissionService.RequireStaffOrAdmin(); return _repository.GetActive(); }
        public Customer? GetById(int id) { PermissionService.RequireStaffOrAdmin(); Require(id > 0, "Id khách hàng không hợp lệ."); return _repository.GetById(id); }
        public List<Customer> Search(string keyword) { PermissionService.RequireStaffOrAdmin(); keyword = Trim(keyword); return string.IsNullOrWhiteSpace(keyword) ? _repository.GetAll() : _repository.Search(keyword); }

        public int Add(Customer customer)
        {
            PermissionService.RequireStaffOrAdmin();
            Validate(customer);

            if (string.IsNullOrWhiteSpace(customer.CustomerCode)) customer.CustomerCode = _repository.GenerateCustomerCode();
            customer.CustomerCode = Trim(customer.CustomerCode).ToUpperInvariant();
            customer.IdentityNumber = TrimNullable(customer.IdentityNumber);
            customer.FullName = Trim(customer.FullName);
            customer.Phone = TrimNullable(customer.Phone);
            customer.Email = TrimNullable(customer.Email);
            customer.Address = TrimNullable(customer.Address);
            customer.IsActive = true;

            if (_repository.IsCustomerCodeExists(customer.CustomerCode)) throw new Exception("Mã khách hàng đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && _repository.IsIdentityNumberExists(customer.IdentityNumber)) throw new Exception("Số CMND/CCCD đã tồn tại.");

            return _repository.Add(customer);
        }

        public bool Update(Customer customer)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(customer.Id > 0, "Id khách hàng không hợp lệ.");
            Validate(customer);

            customer.CustomerCode = Trim(customer.CustomerCode).ToUpperInvariant();
            customer.IdentityNumber = TrimNullable(customer.IdentityNumber);
            customer.FullName = Trim(customer.FullName);
            customer.Phone = TrimNullable(customer.Phone);
            customer.Email = TrimNullable(customer.Email);
            customer.Address = TrimNullable(customer.Address);

            if (_repository.IsCustomerCodeExists(customer.CustomerCode, customer.Id)) throw new Exception("Mã khách hàng đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && _repository.IsIdentityNumberExists(customer.IdentityNumber, customer.Id)) throw new Exception("Số CMND/CCCD đã tồn tại.");

            return _repository.Update(customer);
        }

        public bool SetActive(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id khách hàng không hợp lệ.");
            return _repository.SetActive(id, isActive);
        }

        private void Validate(Customer customer)
        {
            Require(customer != null, "Dữ liệu khách hàng không hợp lệ.");
            Require(!string.IsNullOrWhiteSpace(customer.FullName), "Tên khách hàng không được để trống.");
            Require(customer.FullName.Length <= 150, "Tên khách hàng không được vượt quá 150 ký tự.");
        }
    }
}
