using System.Collections.Generic;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class UserService : ServiceBase
    {
        private readonly UserRepository _userRepo;

        public UserService()
        {
            _userRepo = new UserRepository();
        }

        public void CreateUser(Models.User user, string plainPassword)
        {
            PermissionService.RequireAdmin();
            user.PasswordHash = BookStoreManagement.Helpers.PasswordHasher.HashPassword(plainPassword);
            _userRepo.Add(user);
        }

        public async System.Threading.Tasks.Task CreateUserAsync(Models.User user, string plainPassword)
        {
            PermissionService.RequireAdmin();
            user.PasswordHash = BookStoreManagement.Helpers.PasswordHasher.HashPassword(plainPassword);
            await _userRepo.AddAsync(user);
        }

        public Models.User? GetById(int id)
        {
            PermissionService.RequireAdmin();
            return _userRepo.GetById(id);
        }

        public async System.Threading.Tasks.Task<Models.User?> GetByIdAsync(int id)
        {
            PermissionService.RequireAdmin();
            return await _userRepo.GetByIdAsync(id);
        }

        public void Update(Models.User user)
        {
            PermissionService.RequireAdmin();
            _userRepo.Update(user);
        }

        public async System.Threading.Tasks.Task UpdateAsync(Models.User user)
        {
            PermissionService.RequireAdmin();
            await _userRepo.UpdateAsync(user);
        }

        public void Delete(int userId)
        {
            PermissionService.RequireAdmin();
            _userRepo.Delete(userId);
        }

        public async System.Threading.Tasks.Task DeleteAsync(int userId)
        {
            PermissionService.RequireAdmin();
            await _userRepo.DeleteAsync(userId);
        }

        public void ResetPassword(int userId, string newPlainPassword)
        {
            PermissionService.RequireAdmin();
            string hash = BookStoreManagement.Helpers.PasswordHasher.HashPassword(newPlainPassword);
            _userRepo.ChangePassword(userId, hash);
        }

        public async System.Threading.Tasks.Task ResetPasswordAsync(int userId, string newPlainPassword)
        {
            PermissionService.RequireAdmin();
            string hash = BookStoreManagement.Helpers.PasswordHasher.HashPassword(newPlainPassword);
            await _userRepo.ChangePasswordAsync(userId, hash);
        }

        public (List<AccountItem> Items, int TotalCount) GetPagedAccounts(int page, int pageSize, string searchTerm)
        {
            PermissionService.RequireAdmin();
            return _userRepo.GetPagedAccounts(page, pageSize, searchTerm);
        }

        public async System.Threading.Tasks.Task<(List<AccountItem> Items, int TotalCount)> GetPagedAccountsAsync(int page, int pageSize, string searchTerm)
        {
            PermissionService.RequireAdmin();
            return await _userRepo.GetPagedAccountsAsync(page, pageSize, searchTerm);
        }
    }
}
