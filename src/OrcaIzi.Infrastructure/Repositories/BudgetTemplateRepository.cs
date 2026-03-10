using OrcaIzi.Domain.Core;
using OrcaIzi.Domain.Entities;
using OrcaIzi.Domain.Interfaces;

namespace OrcaIzi.Infrastructure.Repositories
{
    public class BudgetTemplateRepository : Repository<BudgetTemplate>, IBudgetTemplateRepository
    {
        public BudgetTemplateRepository(OrcaIziDbContext context) : base(context) { }

        public async Task<IEnumerable<BudgetTemplate>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<BudgetTemplate?> GetWithItemsAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateWithItemsAsync(BudgetTemplate template)
        {
            var existingItems = await _context.Set<BudgetTemplateItem>()
                .Where(x => x.BudgetTemplateId == template.Id)
                .ToListAsync();

            _context.Set<BudgetTemplateItem>().RemoveRange(existingItems);

            if (_context.Entry(template).State == EntityState.Detached)
            {
                _context.Attach(template);
            }
            _context.Entry(template).State = EntityState.Modified;

            foreach (var item in template.Items)
            {
                var entry = _context.Entry(item);
                entry.State = EntityState.Added;
            }

            await Task.CompletedTask;
        }
    }
}
