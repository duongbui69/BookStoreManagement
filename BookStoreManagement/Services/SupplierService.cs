using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class SupplierService : ServiceBase
    {
        private readonly SupplierRepository _supplierRepository;

        public SupplierService()
        {
            _supplierRepository = new SupplierRepository();
        }

        public List<Supplier> GetAll() => _supplierRepository.GetAll();

        public List<Supplier> GetActive() => _supplierRepository.GetActive();

        public Supplier GetById(int id)
        {
            EnsureId(id, "Id nhà cung cấp");
            return _supplierRepository.GetById(id) ?? throw new Exception("Không tìm thấy nhà cung cấp.");
        }

        public List<Supplier> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _supplierRepository.Search(keyword);
        }

        public int Add(Supplier supplier)
        {
            Validate(supplier);
            NormalizeSupplier(supplier);

            if (_supplierRepository.IsNameExists(supplier.SupplierName))
            {
                throw new Exception("Tên nhà cung cấp đã tồn tại.");
            }

            supplier.IsActive = true;
            return _supplierRepository.Add(supplier);
        }

        public bool Update(Supplier supplier)
        {
            EnsureId(supplier.Id, "Id nhà cung cấp");
            Validate(supplier);
            NormalizeSupplier(supplier);

            if (_supplierRepository.IsNameExists(supplier.SupplierName, supplier.Id))
            {
                throw new Exception("Tên nhà cung cấp đã tồn tại.");
            }

            return _supplierRepository.Update(supplier);
        }

        public bool SetActive(int id, bool isActive)
        {
            EnsureId(id, "Id nhà cung cấp");
            return _supplierRepository.SetActive(id, isActive);
        }

        private void Validate(Supplier supplier)
        {
            if (supplier == null)
            {
                throw new Exception("Dữ liệu nhà cung cấp không hợp lệ.");
            }

            EnsureRequired(supplier.SupplierName, "Tên nhà cung cấp");
            EnsureMaxLength(supplier.SupplierName, 150, "Tên nhà cung cấp");
            EnsureValidPhone(supplier.Phone);
            EnsureValidEmail(supplier.Email);
            EnsureMaxLength(supplier.Phone, 30, "Số điện thoại");
            EnsureMaxLength(supplier.Email, 100, "Email");
            EnsureMaxLength(supplier.Address, 255, "Địa chỉ");
        }

        private void NormalizeSupplier(Supplier supplier)
        {
            supplier.SupplierName = Normalize(supplier.SupplierName);
            supplier.Phone = NormalizeNullable(supplier.Phone);
            supplier.Email = NormalizeNullable(supplier.Email);
            supplier.Address = NormalizeNullable(supplier.Address);
        }
    }
}
