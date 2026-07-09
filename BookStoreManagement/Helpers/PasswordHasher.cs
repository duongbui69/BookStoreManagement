using System;
using System.Security.Cryptography;
using System.Text;

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

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public static bool VerifyPassword(string password, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedPassword))
            {
                return false;
            }

            if (IsBCryptHash(storedPassword))
            {
                return BCrypt.Net.BCrypt.Verify(password, storedPassword);
            }

            // Fallback để bạn còn đăng nhập được nếu dữ liệu mẫu trong SQL đang là mật khẩu plain text.
            // Khi tạo mới hoặc đổi mật khẩu, hệ thống sẽ luôn lưu BCrypt hash.
            return password == storedPassword;
        }

        public static bool IsBCryptHash(string value)
        {
            return value.StartsWith("$2a$", StringComparison.Ordinal)
                || value.StartsWith("$2b$", StringComparison.Ordinal)
                || value.StartsWith("$2y$", StringComparison.Ordinal);
        }

        public static string Sha256ForLegacyOnly(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
