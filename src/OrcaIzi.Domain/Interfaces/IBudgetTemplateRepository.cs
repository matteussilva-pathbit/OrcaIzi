﻿namespace OrcaIzi.Domain.Interfaces
{
    public interface IBudgetTemplateRepository : IRepository<BudgetTemplate>
    {
        Task<IEnumerable<BudgetTemplate>> GetByUserIdAsync(string userId);
        Task<BudgetTemplate?> GetWithItemsAsync(Guid id);
        Task UpdateWithItemsAsync(BudgetTemplate template);
    }
}



