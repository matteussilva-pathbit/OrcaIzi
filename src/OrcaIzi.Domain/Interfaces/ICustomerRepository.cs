using OrcaIzi.Domain.Core;

namespace OrcaIzi.Domain.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> SearchByNameAsync(string name);
        Task<IEnumerable<Customer>> GetByUserIdAsync(string userId);
        Task<PagedResult<Customer>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize);
    }
}
