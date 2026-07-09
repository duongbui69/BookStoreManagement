using System;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class AuthService : ServiceBase
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
            _roleRepository = new RoleRepository();
        }

        public User Login(string username, string password)
        {
            username = Normalize(username);

            EnsureRequired(username, "Tên đăng nhập");
            EnsureRequired(password, "Mật khẩu");

            User? user = _userRepository.GetByUsername(username);

            if (user == null)
            {
                throw new Exception("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            if (!user.IsActive)
            {
                throw new Exception("Tài khoản đã bị khóa.");
            }

            bool validPassword = PasswordHasher.VerifyPassword(password, user.PasswordHash);
            if (!validPassword)
            {
                throw new Exception("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            Role? role = _roleRepository.GetById(user.RoleId);
            if (role == null)
            {
                throw new Exception("Tài khoản chưa có quyền hợp lệ.");
            }

            // Nếu dữ liệu mẫu vẫn lưu plain text, đăng nhập thành công thì tự đổi sang BCrypt.
            if (!PasswordHasher.IsBCryptHash(user.PasswordHash))
            {
                string newHash = PasswordHasher.HashPassword(password);
                _userRepository.ChangePassword(user.Id, newHash);
                user.PasswordHash = newHash;
            }

            CurrentSession.Set(user, role.RoleName);
            return user;
        }

        public void Logout()
        {
            CurrentSession.Clear();
        }

        public bool ChangeMyPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn chưa đăng nhập.");
            }

            EnsureRequired(oldPassword, "Mật khẩu cũ");
            ValidatePassword(newPassword, confirmPassword);

            User? user = _userRepository.GetById(CurrentSession.UserId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy tài khoản hiện tại.");
            }

            if (!PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
            {
                throw new Exception("Mật khẩu cũ không đúng.");
            }

            string newHash = PasswordHasher.HashPassword(newPassword);
            return _userRepository.ChangePassword(user.Id, newHash);
        }

        public void ValidatePassword(string password, string confirmPassword)
        {
            EnsureRequired(password, "Mật khẩu");
            EnsureRequired(confirmPassword, "Xác nhận mật khẩu");

            if (password.Length < 6)
            {
                throw new Exception("Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (password != confirmPassword)
            {
                throw new Exception("Xác nhận mật khẩu không khớp.");
            }
        }
    }
}
