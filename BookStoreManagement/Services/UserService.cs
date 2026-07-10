using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class UserService : ServiceBase
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;

        public UserService()
        {
            _userRepository = new UserRepository();
            _authService = new AuthService();
        }

        public List<UserListViewModel> GetAll()
        {
            PermissionService.RequireAdmin();
            return _userRepository.GetAll();
        }

        public List<UserListViewModel> GetByStoreId(int storeId)
        {
            PermissionService.RequireAdmin();
            Require(storeId > 0, "Id cửa hàng không hợp lệ.");
            return _userRepository.GetByStoreId(storeId);
        }

        public List<UserListViewModel> Search(string keyword)
        {
            PermissionService.RequireAdmin();
            keyword = Trim(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? _userRepository.GetAll() : _userRepository.Search(keyword);
        }

        public User? GetById(int id)
        {
            PermissionService.RequireSelfOrAdmin(id);
            Require(id > 0, "Id tài khoản không hợp lệ.");
            return _userRepository.GetById(id);
        }

        public int Add(User user, string rawPassword, string confirmPassword)
        {
            PermissionService.RequireAdmin();
            ValidateUser(user, true);
            _authService.ValidatePassword(rawPassword, confirmPassword);

            if (string.IsNullOrWhiteSpace(user.UserCode)) user.UserCode = _userRepository.GenerateUserCode();
            NormalizeUser(user);

            if (_userRepository.IsUserCodeExists(user.UserCode)) throw new Exception("Mã nhân viên đã tồn tại.");
            if (_userRepository.IsUsernameExists(user.Username)) throw new Exception("Tên đăng nhập đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(user.IdentityNumber) && _userRepository.IsIdentityNumberExists(user.IdentityNumber)) throw new Exception("Số CMND/CCCD đã tồn tại.");

            user.PasswordHash = PasswordHasher.HashPassword(rawPassword);
            user.IsActive = true;

            return _userRepository.Add(user);
        }

        public bool Update(User user)
        {
            PermissionService.RequireAdmin();
            Require(user.Id > 0, "Id tài khoản không hợp lệ.");
            ValidateUser(user, false);
            NormalizeUser(user);

            if (_userRepository.IsUserCodeExists(user.UserCode, user.Id)) throw new Exception("Mã nhân viên đã tồn tại.");
            if (_userRepository.IsUsernameExists(user.Username, user.Id)) throw new Exception("Tên đăng nhập đã tồn tại.");
            if (!string.IsNullOrWhiteSpace(user.IdentityNumber) && _userRepository.IsIdentityNumberExists(user.IdentityNumber, user.Id)) throw new Exception("Số CMND/CCCD đã tồn tại.");

            return _userRepository.Update(user);
        }

        public bool ResetPassword(int userId, string newPassword, string confirmPassword)
        {
            PermissionService.RequireAdmin();
            Require(userId > 0, "Id tài khoản không hợp lệ.");
            _authService.ValidatePassword(newPassword, confirmPassword);
            return _userRepository.ChangePassword(userId, PasswordHasher.HashPassword(newPassword));
        }

        public bool SetActive(int userId, bool isActive)
        {
            PermissionService.RequireAdmin();
            Require(userId > 0, "Id tài khoản không hợp lệ.");
            if (userId == CurrentSession.UserId && !isActive) throw new Exception("Không thể tự khóa tài khoản đang đăng nhập.");
            return _userRepository.SetActive(userId, isActive);
        }

        private void ValidateUser(User user, bool isCreate)
        {
            Require(user != null, "Dữ liệu tài khoản không hợp lệ.");
            Require(user.RoleId > 0, "Vai trò không hợp lệ.");
            Require(!string.IsNullOrWhiteSpace(user.Username), "Tên đăng nhập không được để trống.");
            Require(!string.IsNullOrWhiteSpace(user.FullName), "Họ tên không được để trống.");
            Require(user.Username.Length <= 50, "Tên đăng nhập không được vượt quá 50 ký tự.");
            Require(user.FullName.Length <= 150, "Họ tên không được vượt quá 150 ký tự.");
        }

        private void NormalizeUser(User user)
        {
            user.UserCode = Trim(user.UserCode).ToUpperInvariant();
            user.Username = Trim(user.Username);
            user.FullName = Trim(user.FullName);
            user.IdentityNumber = TrimNullable(user.IdentityNumber);
            user.Phone = TrimNullable(user.Phone);
            user.Email = TrimNullable(user.Email);
            user.Address = TrimNullable(user.Address);
        }
    }
}
