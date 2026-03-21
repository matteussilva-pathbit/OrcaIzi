﻿namespace OrcaIzi.Domain.Interfaces
{
    public interface ICatalogItemRepository : IRepository<CatalogItem>
    {
        Task<IEnumerable<CatalogItem>> GetByUserIdAsync(string userId, bool onlyActive);
        Task<PagedResult<CatalogItem>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize, string? search, bool onlyActive);
    }
}



