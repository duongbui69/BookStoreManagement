using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class UserService : ServiceBase
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;
        private readonly AuthService _authService;

        public UserService()
        {
            _userRepository = new UserRepository();
            _roleRepository = new RoleRepository();
            _authService = new AuthService();
        }

        public List<User> GetAll() => _userRepository.GetAll();

        public User GetById(int id)
        {
            EnsureId(id, "Id tài khoản");
            return _userRepository.GetById(id) ?? throw new Exception("Không tìm thấy tài khoản.");
        }

        public User GetByUsername(string username)
        {
            username = Normalize(username);
            EnsureRequired(username, "Tên đăng nhập");
            return _userRepository.GetByUsername(username) ?? throw new Exception("Không tìm thấy tài khoản.");
        }

        public List<User> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _userRepository.Search(keyword);
        }

        public int Add(User user, string password, string confirmPassword)
        {
            ValidateUser(user, isCreate: true);
            _authService.ValidatePassword(password, confirmPassword);
            NormalizeUser(user);

            if (_userRepository.IsUsernameExists(user.Username))
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(user.Email) && _userRepository.IsEmailExists(user.Email))
            {
                throw new Exception("Email đã tồn tại.");
            }

            if (_roleRepository.GetById(user.RoleId) == null)
            {
                throw new Exception("Role không hợp lệ.");
            }

            user.PasswordHash = PasswordHasher.HashPassword(password);
            user.IsActive = true;

            return _userRepository.Add(user);
        }

        public bool Update(User user)
        {
            EnsureId(user.Id, "Id tài khoản");
            ValidateUser(user, isCreate: false);
            NormalizeUser(user);

            if (_userRepository.IsUsernameExists(user.Username, user.Id))
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(user.Email) && _userRepository.IsEmailExists(user.Email, user.Id))
            {
                throw new Exception("Email đã tồn tại.");
            }

            if (_roleRepository.GetById(user.RoleId) == null)
            {
                throw new Exception("Role không hợp lệ.");
            }

            return _userRepository.Update(user);
        }

        public bool ResetPassword(int userId, string newPassword, string confirmPassword)
        {
            EnsureId(userId, "Id tài khoản");
            _authService.ValidatePassword(newPassword, confirmPassword);

            string hash = PasswordHasher.HashPassword(newPassword);
            return _userRepository.ChangePassword(userId, hash);
        }

        public bool SetActive(int id, bool isActive)
        {
            EnsureId(id, "Id tài khoản");

            if (CurrentSession.IsLoggedIn && CurrentSession.UserId == id && !isActive)
            {
                throw new Exception("Không thể tự khóa tài khoản đang đăng nhập.");
            }

            return _userRepository.SetActive(id, isActive);
        }

        private void ValidateUser(User user, bool isCreate)
        {
            if (user == null)
            {
                throw new Exception("Dữ liệu tài khoản không hợp lệ.");
            }

            EnsureId(user.RoleId, "Role");
            EnsureRequired(user.Username, "Tên đăng nhập");
            EnsureRequired(user.FullName, "Họ tên");
            EnsureMaxLength(user.Username, 50, "Tên đăng nhập");
            EnsureMaxLength(user.FullName, 150, "Họ tên");
            EnsureMaxLength(user.Phone, 30, "Số điện thoại");
            EnsureMaxLength(user.Email, 100, "Email");
            EnsureMaxLength(user.Address, 255, "Địa chỉ");
            EnsureValidPhone(user.Phone);
            EnsureValidEmail(user.Email);

            if (user.Username.Contains(" "))
            {
                throw new Exception("Tên đăng nhập không được chứa khoảng trắng.");
            }
        }

        private void NormalizeUser(User user)
        {
            user.Username = Normalize(user.Username);
            user.FullName = Normalize(user.FullName);
            user.Phone = NormalizeNullable(user.Phone);
            user.Email = NormalizeNullable(user.Email);
            user.Address = NormalizeNullable(user.Address);
        }
    }
}
