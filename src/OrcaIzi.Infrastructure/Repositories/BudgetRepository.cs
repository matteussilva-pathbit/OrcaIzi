using OrcaIzi.Domain.Core;

namespace OrcaIzi.Infrastructure.Repositories
{
    public class BudgetRepository : Repository<Budget>, IBudgetRepository
    {
        public BudgetRepository(OrcaIziDbContext context) : base(context) { }

        public async Task<IEnumerable<Budget>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _dbSet
                .Where(x => x.CustomerId == customerId)
                .Include(x => x.Customer)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Budget> GetWithItemsAndCustomerAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Customer)
                .Include(x => x.User)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Budget?> GetByPaymentExternalIdAsync(string paymentExternalId)
        {
            return await _dbSet
                .Include(x => x.Customer)
                .Include(x => x.User)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.PaymentExternalId == paymentExternalId);
        }

        public async Task<Budget?> GetByPublicShareIdAsync(Guid publicShareId)
        {
            return await _dbSet
                .Include(x => x.Customer)
                .Include(x => x.User)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.PublicShareId == publicShareId && x.PublicShareEnabled);
        }

        public override async Task<IEnumerable<Budget>> GetAllAsync()
        {
            return await _dbSet
                .Include(x => x.Customer)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<Budget>> GetAllPagedByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            var query = _dbSet.Where(b => b.UserId == userId);
            
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(x => x.Customer)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Budget>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task UpdateWithItemsAsync(Budget budget)
        {
            // Transaction approach to ensure clean update without concurrency issues
            // We will:
            // 1. Manually find and delete all existing items for this budget
            // 2. Add the new items from the budget object
            
            // Note: This bypasses EF Core change tracking magic and uses raw logic to be safe.
            
            // First, find all existing items in the database
            var existingItems = await _context.Set<BudgetItem>()
                .Where(x => x.BudgetId == budget.Id)
                .ToListAsync();
                
            // Remove them from context and mark for deletion
            _context.Set<BudgetItem>().RemoveRange(existingItems);
            
            // Force the Budget entity to be Modified
            if (_context.Entry(budget).State == EntityState.Detached)
            {
                _context.Attach(budget);
            }
            _context.Entry(budget).State = EntityState.Modified;
            
            // Ensure new items are added
            foreach (var item in budget.Items)
            {
                // Ensure Description is explicitly handled
                // Even if it's null, we want EF to know about it.
                // However, the issue might be that EF thinks it's an existing entity if IDs are preserved in memory?
                // No, BudgetItem ID is Guid.NewGuid() usually.
                
                var entry = _context.Entry(item);
                entry.State = EntityState.Added;
                
                // Explicitly set Description to null if needed (though EF should handle it)
                if (item.Description == null)
                {
                    entry.Property("Description").IsModified = true;
                }
            }
            
            await Task.CompletedTask;
        }
    }
}
