using OrcaIzi.Domain.Core;

namespace OrcaIzi.Infrastructure.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(OrcaIziDbContext context) : base(context) { }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<IEnumerable<Customer>> SearchByNameAsync(string name)
        {
            return await _dbSet
                .Where(x => x.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<PagedResult<Customer>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            var query = _dbSet.Where(x => x.UserId == userId);
            
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
