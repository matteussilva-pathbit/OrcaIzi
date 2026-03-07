using OrcaIzi.Domain.Core;

namespace OrcaIzi.Application.Interfaces
{
    public interface IBudgetAppService
    {
        Task<BudgetDto> GetByIdAsync(Guid id);
        Task<IEnumerable<BudgetDto>> GetAllAsync();
        Task<PagedResult<BudgetDto>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<BudgetDto> CreateAsync(CreateBudgetDto budgetDto);
        Task<BudgetDto> UpdateAsync(Guid id, CreateBudgetDto budgetDto);
        Task UpdateStatusAsync(Guid id, string status);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<BudgetDto>> GetByCustomerIdAsync(Guid customerId);
        Task<byte[]> GeneratePdfAsync(Guid id);
        Task<DashboardDto> GetDashboardStatsAsync();
    }
}
