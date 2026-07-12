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

        public (List<CatalogBookItem> Items, int TotalCount) GetPagedCatalogBooks(int page, int pageSize, string searchTerm = "")
        {
            return _repo.GetPagedCatalogBooks(page, pageSize, searchTerm);
        }
    }
}
