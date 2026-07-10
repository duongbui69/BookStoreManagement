using System;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class AuthService : ServiceBase
    {
        private readonly UserRepository _userRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        public User? Login(string username, string password)
        {
            username = Trim(username);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            User? user = _userRepository.GetByUsername(username);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            if (PasswordHasher.NeedsRehash(user.PasswordHash))
            {
                string newHash = PasswordHasher.HashPassword(password);
                _userRepository.ChangePassword(user.Id, newHash);
                user.PasswordHash = newHash;
            }

            CurrentSession.SetCurrentUser(user);
            return user;
        }

        public void Logout()
        {
            CurrentSession.Clear();
        }

        public bool ChangeMyPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            PermissionService.RequireLogin();

            User? user = _userRepository.GetById(CurrentSession.UserId);
            Require(user != null, "Không tìm thấy tài khoản hiện tại.");

            if (!PasswordHasher.VerifyPassword(oldPassword, user!.PasswordHash))
            {
                throw new Exception("Mật khẩu cũ không đúng.");
            }

            ValidatePassword(newPassword, confirmPassword);

            return _userRepository.ChangePassword(CurrentSession.UserId, PasswordHasher.HashPassword(newPassword));
        }

        public void ValidatePassword(string password, string? confirmPassword = null)
        {
            Require(!string.IsNullOrWhiteSpace(password), "Mật khẩu không được để trống.");
            Require(password.Length >= 6, "Mật khẩu phải có ít nhất 6 ký tự.");

            if (confirmPassword != null)
            {
                Require(password == confirmPassword, "Mật khẩu xác nhận không khớp.");
            }
        }
    }
}
