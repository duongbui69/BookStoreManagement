using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class RoleService : ServiceBase
    {
        private readonly RoleRepository _repository;
        public RoleService() { _repository = new RoleRepository(); }
        public List<Role> GetAll() { PermissionService.RequireAdmin(); return _repository.GetAll(); }
        public Role? GetById(int id) { PermissionService.RequireAdmin(); Require(id > 0, "Id quyền không hợp lệ."); return _repository.GetById(id); }
        public Role? GetByName(string roleName) { PermissionService.RequireAdmin(); roleName = Trim(roleName); Require(!string.IsNullOrWhiteSpace(roleName), "Tên quyền không được để trống."); return _repository.GetByName(roleName); }

        public async System.Threading.Tasks.Task<List<Role>> GetAllAsync() 
        { 
            PermissionService.RequireAdmin(); 
            return await _repository.GetAllAsync(); 
        }

        public async System.Threading.Tasks.Task<Role?> GetByIdAsync(int id) 
        { 
            PermissionService.RequireAdmin(); 
            Require(id > 0, "Id quyền không hợp lệ."); 
            return await _repository.GetByIdAsync(id); 
        }

        public async System.Threading.Tasks.Task<Role?> GetByNameAsync(string roleName) 
        { 
            PermissionService.RequireAdmin(); 
            roleName = Trim(roleName); 
            Require(!string.IsNullOrWhiteSpace(roleName), "Tên quyền không được để trống."); 
            return await _repository.GetByNameAsync(roleName); 
        }
    }
}
