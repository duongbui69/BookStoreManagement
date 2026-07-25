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
            Require(id > 0, "Invalid customer ID."); 
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

            if (await _repository.IsCustomerCodeExistsAsync(customer.CustomerCode)) throw new Exception("Customer code already exists.");
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && await _repository.IsIdentityNumberExistsAsync(customer.IdentityNumber)) throw new Exception("ID Card already exists.");

            return await _repository.AddAsync(customer);
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(customer.Id > 0, "Invalid customer ID.");
            Validate(customer);

            customer.CustomerCode = Trim(customer.CustomerCode).ToUpperInvariant();
            customer.IdentityNumber = TrimNullable(customer.IdentityNumber);
            customer.FullName = Trim(customer.FullName);
            customer.Phone = TrimNullable(customer.Phone);
            customer.Email = TrimNullable(customer.Email);
            customer.Address = TrimNullable(customer.Address);

            if (await _repository.IsCustomerCodeExistsAsync(customer.CustomerCode, customer.Id)) throw new Exception("Customer code already exists.");
            if (!string.IsNullOrWhiteSpace(customer.IdentityNumber) && await _repository.IsIdentityNumberExistsAsync(customer.IdentityNumber, customer.Id)) throw new Exception("ID Card already exists.");

            return await _repository.UpdateAsync(customer);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Invalid customer ID.");
            return await _repository.SetActiveAsync(id, isActive);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            PermissionService.RequireAdmin();
            Require(id > 0, "Invalid customer ID.");
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<int> ids)
        {
            PermissionService.RequireAdmin();
            Require(ids != null, "Invalid ID list.");
            return await _repository.DeleteMultipleAsync(ids);
        }

        private void Validate(Customer customer)
        {
            Require(customer != null, "Invalid customer data.");
            Require(!string.IsNullOrWhiteSpace(customer.FullName), "Customer name cannot be empty.");
            Require(customer.FullName.Length <= 150, "Customer name cannot exceed 150 chars.");
            Require(customer.Points >= 0, "Reward points cannot be < 0.");
        }
    }
}
