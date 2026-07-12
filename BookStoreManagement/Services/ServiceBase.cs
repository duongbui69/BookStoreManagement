using System;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Services
{
    public abstract class ServiceBase
    {
        protected ServiceBase()
        {
        }

        protected string Trim(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        protected string? TrimNullable(string? value)
        {
            string trimmed = value?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        protected void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }
    }
}
