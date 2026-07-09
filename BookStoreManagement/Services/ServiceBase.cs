using System;
using System.Text.RegularExpressions;

namespace BookStoreManagement.Services
{
    public abstract class ServiceBase
    {
        protected static string Normalize(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        protected static string? NormalizeNullable(string? value)
        {
            value = value?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        protected static void EnsureId(int id, string fieldName)
        {
            if (id <= 0)
            {
                throw new Exception($"{fieldName} không hợp lệ.");
            }
        }

        protected static void EnsureRequired(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"{fieldName} không được để trống.");
            }
        }

        protected static void EnsureMaxLength(string? value, int maxLength, string fieldName)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                throw new Exception($"{fieldName} không được vượt quá {maxLength} ký tự.");
            }
        }

        protected static void EnsureNonNegative(decimal value, string fieldName)
        {
            if (value < 0)
            {
                throw new Exception($"{fieldName} không được nhỏ hơn 0.");
            }
        }

        protected static void EnsureNonNegative(int value, string fieldName)
        {
            if (value < 0)
            {
                throw new Exception($"{fieldName} không được nhỏ hơn 0.");
            }
        }

        protected static void EnsurePositive(int value, string fieldName)
        {
            if (value <= 0)
            {
                throw new Exception($"{fieldName} phải lớn hơn 0.");
            }
        }

        protected static void EnsurePositive(decimal value, string fieldName)
        {
            if (value <= 0)
            {
                throw new Exception($"{fieldName} phải lớn hơn 0.");
            }
        }

        protected static void EnsureValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email.Trim(), pattern))
            {
                throw new Exception("Email không đúng định dạng.");
            }
        }

        protected static void EnsureValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return;
            }

            string pattern = @"^[0-9+\-\s]{8,20}$";
            if (!Regex.IsMatch(phone.Trim(), pattern))
            {
                throw new Exception("Số điện thoại không đúng định dạng.");
            }
        }
    }
}
