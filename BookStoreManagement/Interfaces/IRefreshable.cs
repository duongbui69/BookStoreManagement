using System.Threading.Tasks;

namespace BookStoreManagement.Interfaces
{
    public interface IRefreshable
    {
        Task RefreshDataAsync();
    }
}
