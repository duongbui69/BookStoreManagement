namespace BookStoreManagement.Models
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Staff = "Staff";
        }

        public static class PaymentMethods
        {
            public const string Cash = "Cash";
            public const string Banking = "Banking";
            public const string Card = "Card";
        }

        public static class OrderStatuses
        {
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
        }

        public static class InventoryTransactionTypes
        {
            public const string Import = "Import";
            public const string Sale = "Sale";
            public const string Adjustment = "Adjustment";
            public const string CancelSale = "CancelSale";
            public const string Return = "Return";
        }

        public static class ReferenceTypes
        {
            public const string SalesOrder = "SalesOrder";
            public const string PurchaseReceipt = "PurchaseReceipt";
            public const string ReturnReceipt = "ReturnReceipt";
            public const string Manual = "Manual";
        }
    }
}
