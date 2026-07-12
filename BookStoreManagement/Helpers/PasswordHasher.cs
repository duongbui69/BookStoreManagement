using System;
using BCrypt.Net;

namespace BookStoreManagement.Helpers
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Mật khẩu không được để trống.");
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            // Hỗ trợ database mẫu đang lưu plain text như 123456.
            // Sau khi đăng nhập thành công, AuthService có thể tự nâng cấp sang BCrypt.
            if (!storedPassword.StartsWith("$2", StringComparison.Ordinal))
            {
                return password == storedPassword;
            }

            return BCrypt.Net.BCrypt.Verify(password, storedPassword);
        }

        public static bool NeedsRehash(string storedPassword)
        {
            return string.IsNullOrWhiteSpace(storedPassword)
                   || !storedPassword.StartsWith("$2", StringComparison.Ordinal);
        }
    }
}
