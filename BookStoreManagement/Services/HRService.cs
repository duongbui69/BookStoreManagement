using System.Collections.Generic;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class HRService : ServiceBase
    {
        private readonly HRRepository _repo;
        private readonly UserRepository _userRepo;

        public HRService()
        {
            _repo = new HRRepository();
            _userRepo = new UserRepository();
        }

        public void CreateUser(Models.User user, string plainPassword)
        {
            PermissionService.RequireAdmin();
            user.PasswordHash = BookStoreManagement.Helpers.PasswordHasher.HashPassword(plainPassword);
            _userRepo.Add(user);
        }

        public Models.User? GetById(int id)
        {
            PermissionService.RequireAdmin();
            return _userRepo.GetById(id);
        }

        public void UpdateUser(Models.User user)
        {
            PermissionService.RequireAdmin();
            _userRepo.Update(user);
        }

        public void DeleteUser(int userId)
        {
            PermissionService.RequireAdmin();
            _userRepo.Delete(userId);
        }

        public void ResetPassword(int userId, string newPlainPassword)
        {
            PermissionService.RequireAdmin();
            string hash = BookStoreManagement.Helpers.PasswordHasher.HashPassword(newPlainPassword);
            _userRepo.ChangePassword(userId, hash);
        }

        public HRStats GetStats()
        {
            return _repo.GetStats();
        }

        public (List<HREmployeeItem> Items, int TotalCount) GetPagedEmployees(int page, int pageSize, string departmentFilter, string statusFilter, string searchTerm)
        {
            return _repo.GetPagedEmployees(page, pageSize, departmentFilter, statusFilter, searchTerm);
        }

        public async System.Threading.Tasks.Task<HRStats> GetStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }

        public async System.Threading.Tasks.Task<(List<HREmployeeItem> Items, int TotalCount)> GetPagedEmployeesAsync(int page, int pageSize, string departmentFilter, string statusFilter, string searchTerm)
        {
            return await _repo.GetPagedEmployeesAsync(page, pageSize, departmentFilter, statusFilter, searchTerm);
        }
    }
}
