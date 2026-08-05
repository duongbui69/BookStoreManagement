using System;

namespace BookStoreManagement.Events
{
    public static class GlobalEvents
    {
        public static event Action TransactionCompleted;
        public static event Action RequestMyInvoicesView;

        public static void OnTransactionCompleted()
        {
            TransactionCompleted?.Invoke();
        }

        public static void OnRequestMyInvoicesView()
        {
            RequestMyInvoicesView?.Invoke();
        }
    }
}
