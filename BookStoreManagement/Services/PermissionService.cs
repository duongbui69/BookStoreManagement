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
                throw new Exception("You are not logged in.");
            }
        }

        public void RequireAdmin()
        {
            RequireLogin();

            if (!CurrentSession.IsAdmin)
            {
                throw new Exception("This function is for Admin only.");
            }
        }

        public void RequireStaffOrAdmin()
        {
            RequireLogin();

            if (!CurrentSession.IsAdmin && !CurrentSession.IsStaff)
            {
                throw new Exception("You do not have permission to use this function.");
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
                throw new Exception("You can only manipulate data from your store.");
            }
        }

        public void RequireSelfOrAdmin(int userId)
        {
            RequireLogin();

            if (!CurrentSession.IsAdmin && CurrentSession.UserId != userId)
            {
                throw new Exception("You do not have permission to view or edit this information.");
            }
        }
    }
}
