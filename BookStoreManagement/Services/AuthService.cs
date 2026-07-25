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
            Require(CurrentSession.UserId > 0, "Please log in.");

            User? user = _userRepository.GetById(CurrentSession.UserId);
            Require(user != null, "Current account not found.");

            if (!PasswordHasher.VerifyPassword(oldPassword, user!.PasswordHash))
            {
                throw new Exception("Old password is incorrect.");
            }

            ValidatePassword(newPassword, confirmPassword);

            return _userRepository.ChangePassword(CurrentSession.UserId, PasswordHasher.HashPassword(newPassword));
        }

        public void ValidatePassword(string password, string? confirmPassword = null)
        {
            Require(!string.IsNullOrWhiteSpace(password), "Password cannot be empty.");
            Require(password.Length >= 6, "Password must have at least 6 characters.");

            if (confirmPassword != null)
            {
                Require(password == confirmPassword, "Confirmation password does not match.");
            }
        }
    }
}
