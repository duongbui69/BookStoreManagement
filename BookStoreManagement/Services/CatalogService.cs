using System.Collections.Generic;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class CatalogService : ServiceBase
    {
        private readonly CatalogRepository _repo;

        public CatalogService()
        {
            _repo = new CatalogRepository();
        }

        public CatalogStats GetStats()
        {
            return _repo.GetStats();
        }

        public async System.Threading.Tasks.Task<CatalogStats> GetStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }

        public (List<CatalogBookItem> Items, int TotalCount) GetPagedCatalogBooks(int page, int pageSize, string searchTerm = "", int? categoryId = null, string stockStatus = "")
        {
            return _repo.GetPagedCatalogBooks(page, pageSize, searchTerm, categoryId, stockStatus);
        }

        public async System.Threading.Tasks.Task<(List<CatalogBookItem> Items, int TotalCount)> GetPagedCatalogBooksAsync(int page, int pageSize, string searchTerm = "", int? categoryId = null, string stockStatus = "")
        {
            return await _repo.GetPagedCatalogBooksAsync(page, pageSize, searchTerm, categoryId, stockStatus);
        }
    }
}
