using System;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Services
{
    public class PermissionService
    {
        public void RequireLogin()
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn chưa đăng nhập.");
            }
        }

        public void RequireAdmin()
        {
            RequireLogin();

            if (!CurrentSession.IsAdmin)
            {
                throw new Exception("Chức năng này chỉ dành cho Admin.");
            }
        }

        public void RequireStaffOrAdmin()
        {
            RequireLogin();

            if (!CurrentSession.IsAdmin && !CurrentSession.IsStaff)
            {
                throw new Exception("Bạn không có quyền sử dụng chức năng này.");
            }
        }

        public void RequireSameStoreOrAdmin(int storeId)
        {
            RequireStaffOrAdmin();

            if (CurrentSession.IsAdmin)
            {
                return;
            }

            if (!CurrentSession.StoreId.HasValue || CurrentSession.StoreId.Value != storeId)
            {
                throw new Exception("Bạn chỉ được thao tác với dữ liệu của cửa hàng mình.");
            }
        }

        public void RequireSelfOrAdmin(int userId)
        {
            RequireLogin();

            if (!CurrentSession.IsAdmin && CurrentSession.UserId != userId)
            {
                throw new Exception("Bạn không có quyền xem hoặc sửa thông tin này.");
            }
        }
    }
}
