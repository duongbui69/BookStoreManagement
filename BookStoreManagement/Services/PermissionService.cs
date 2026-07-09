using System;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;

namespace BookStoreManagement.Services
{
    public class PermissionService
    {
        public bool IsAdmin()
        {
            return CurrentSession.RoleName == AppConstants.Roles.Admin;
        }

        public bool IsStaff()
        {
            return CurrentSession.RoleName == AppConstants.Roles.Staff;
        }

        public void RequireLogin()
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn cần đăng nhập để sử dụng chức năng này.");
            }
        }

        public void RequireAdmin()
        {
            RequireLogin();

            if (!IsAdmin())
            {
                throw new Exception("Bạn không có quyền Admin để sử dụng chức năng này.");
            }
        }

        public void RequireStaffOrAdmin()
        {
            RequireLogin();

            if (!IsAdmin() && !IsStaff())
            {
                throw new Exception("Bạn không có quyền sử dụng chức năng này.");
            }
        }

        public bool CanManageUsers() => IsAdmin();
        public bool CanManageBooks() => IsAdmin();
        public bool CanManageCategories() => IsAdmin();
        public bool CanManageAuthors() => IsAdmin();
        public bool CanManagePublishers() => IsAdmin();
        public bool CanManageSuppliers() => IsAdmin();
        public bool CanCreatePurchaseReceipt() => IsAdmin();
        public bool CanViewRevenueReport() => IsAdmin();
        public bool CanAdjustInventory() => IsAdmin();
        public bool CanCreateSalesOrder() => IsAdmin() || IsStaff();
        public bool CanViewAllSalesOrders() => IsAdmin();
    }
}
