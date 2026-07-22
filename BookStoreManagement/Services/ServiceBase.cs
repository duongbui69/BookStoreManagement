using System;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Services
{
    public abstract class ServiceBase
    {
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

        protected readonly PermissionService PermissionService;

        protected ServiceBase()
        {
            PermissionService = new PermissionService();
        }

        protected int ResolveStoreIdForWrite(int requestedStoreId)
        {
            PermissionService.RequireStaffOrAdmin();

            if (CurrentSession.IsAdmin)
            {
                Require(requestedStoreId > 0, "Cửa hàng không hợp lệ.");
                return requestedStoreId;
            }

            int storeIdToUse = CurrentSession.StoreId ?? 1;

            if (requestedStoreId > 0 && requestedStoreId != storeIdToUse)
            {
                throw new Exception("Nhân viên chỉ được thao tác tại cửa hàng của mình.");
            }

            return storeIdToUse;
        }

        protected int? ResolveStoreIdForRead(int? requestedStoreId)
        {
            PermissionService.RequireStaffOrAdmin();

            if (CurrentSession.IsAdmin)
            {
                return requestedStoreId;
            }

            int storeIdToUse = CurrentSession.StoreId ?? 1;

            if (requestedStoreId.HasValue && requestedStoreId.Value != storeIdToUse)
            {
                throw new Exception("Nhân viên chỉ được xem dữ liệu của cửa hàng mình.");
            }

            return storeIdToUse;
        }
    }
}
