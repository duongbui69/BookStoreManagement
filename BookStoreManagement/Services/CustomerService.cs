using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class CustomerService : ServiceBase
    {
        private readonly CustomerRepository _repository;
        public CustomerService() { _repository = new CustomerRepository(); }

        public async Task<List<Customer>> GetAllAsync() { PermissionService.RequireStaffOrAdmin(); return await _repository.GetAllAsync(); }
        public async Task<List<Customer>> GetActiveAsync() { PermissionService.RequireStaffOrAdmin(); return await _repository.GetActiveAsync(); }
        
        public async Task<Customer?> GetByIdAsync(int id) 
        { 
            PermissionService.RequireStaffOrAdmin(); 
            Require(id > 0, "Id khách hàng không hợp lệ."); 
            return await _repository.GetByIdAsync(id); 
        }

        public async Task<List<Customer>> SearchAsync(string keyword) 
        { 
            PermissionService.RequireStaffOrAdmin(); 
            keyword = Trim(keyword); 
            return string.IsNullOrWhiteSpace(keyword) ? await _repository.GetAllAsync() : await _repository.SearchAsync(keyword); 
        }

        public async Task<int> AddAsync(Customer customer)
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

            if (await _repository.IsCustomerCodeExistsAsync(customer.CustomerCode)) throw new Exception("Mã khách hàng đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && await _repository.IsIdentityNumberExistsAsync(customer.IdentityNumber)) throw new Exception("Số CMND/CCCD đã tồn tại.");

            return await _repository.AddAsync(customer);
        }

        public async Task<bool> UpdateAsync(Customer customer)
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

            if (await _repository.IsCustomerCodeExistsAsync(customer.CustomerCode, customer.Id)) throw new Exception("Mã khách hàng đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && await _repository.IsIdentityNumberExistsAsync(customer.IdentityNumber, customer.Id)) throw new Exception("Số CMND/CCCD đã tồn tại.");

            return await _repository.UpdateAsync(customer);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id khách hàng không hợp lệ.");
            return await _repository.SetActiveAsync(id, isActive);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Id khách hàng không hợp lệ.");
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<int> ids)
        {
            PermissionService.RequireAdmin();
            Require(ids != null, "Danh sách Id không hợp lệ.");
            return await _repository.DeleteMultipleAsync(ids);
        }

        private void Validate(Customer customer)
        {
            Require(customer != null, "Dữ liệu khách hàng không hợp lệ.");
            Require(!string.IsNullOrWhiteSpace(customer.FullName), "Tên khách hàng không được để trống.");
            Require(customer.FullName.Length <= 150, "Tên khách hàng không được vượt quá 150 ký tự.");
            Require(customer.Points >= 0, "Điểm tích lũy không được nhỏ hơn 0.");
        }
    }
}
