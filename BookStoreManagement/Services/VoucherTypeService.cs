using System;
using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Services
{
    public class VoucherTypeService : ServiceBase
    {
        private readonly VoucherTypeRepository _repository;
        public VoucherTypeService() { _repository = new VoucherTypeRepository(); }

        public List<VoucherType> GetAll() => _repository.GetAll();

        public VoucherType? GetById(int id) => _repository.GetById(id);

        public void Save(VoucherType vt)
        {
            Require(!string.IsNullOrWhiteSpace(vt.Code), "Mã loại không được để trống.");
            Require(!string.IsNullOrWhiteSpace(vt.Name), "Tên loại không được để trống.");

            if (vt.Id == 0)
                _repository.Insert(vt);
            else
                _repository.Update(vt);
        }

        public void Delete(int id)
        {
            Require(id > 0, "Id không hợp lệ.");
            _repository.Delete(id);
        }

        public async System.Threading.Tasks.Task<List<VoucherType>> GetAllAsync() => await _repository.GetAllAsync();

        public async System.Threading.Tasks.Task<VoucherType?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async System.Threading.Tasks.Task SaveAsync(VoucherType vt)
        {
            Require(!string.IsNullOrWhiteSpace(vt.Code), "Mã loại không được để trống.");
            Require(!string.IsNullOrWhiteSpace(vt.Name), "Tên loại không được để trống.");

            if (vt.Id == 0)
                await _repository.InsertAsync(vt);
            else
                await _repository.UpdateAsync(vt);
        }

        public async System.Threading.Tasks.Task DeleteAsync(int id)
        {
            Require(id > 0, "Id không hợp lệ.");
            await _repository.DeleteAsync(id);
        }
    }
}
