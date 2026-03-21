﻿namespace OrcaIzi.Infrastructure.Repositories
{
    public class CatalogItemRepository : Repository<CatalogItem>, ICatalogItemRepository
    {
        public CatalogItemRepository(OrcaIziDbContext context) : base(context) { }

        public async Task<IEnumerable<CatalogItem>> GetByUserIdAsync(string userId, bool onlyActive)
        {
            var query = _dbSet.Where(x => x.UserId == userId);
            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<PagedResult<CatalogItem>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize, string? search, bool onlyActive)
        {
            var query = _dbSet.Where(x => x.UserId == userId);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search) || (x.Category != null && x.Category.Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CatalogItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}



