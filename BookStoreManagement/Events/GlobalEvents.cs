using System;

namespace BookStoreManagement.Events
{
    public static class GlobalEvents
    {
        public static event Action TransactionCompleted;

        public static void OnTransactionCompleted()
        {
            TransactionCompleted?.Invoke();
        }
    }
}
