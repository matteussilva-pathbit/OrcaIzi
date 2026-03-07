using OrcaIzi.Domain.Core;

namespace OrcaIzi.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<PagedResult<T>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
