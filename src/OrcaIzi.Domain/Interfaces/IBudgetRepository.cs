using OrcaIzi.Domain.Core;

namespace OrcaIzi.Domain.Interfaces
{
    public interface IBudgetRepository : IRepository<Budget>
    {
        Task<IEnumerable<Budget>> GetByCustomerIdAsync(Guid customerId);
        Task<Budget> GetWithItemsAndCustomerAsync(Guid id);
        Task<Budget?> GetByPaymentExternalIdAsync(string paymentExternalId);
        Task<Budget?> GetByPublicShareIdAsync(Guid publicShareId);
        Task<PagedResult<Budget>> GetAllPagedByUserIdAsync(string userId, int pageNumber, int pageSize);
        Task UpdateWithItemsAsync(Budget budget);
    }
}
