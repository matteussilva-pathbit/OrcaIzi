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
        Task<BudgetDto> DuplicateAsync(Guid id);
        Task UpdateStatusAsync(Guid id, string status);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<BudgetDto>> GetByCustomerIdAsync(Guid customerId);
        Task<byte[]> GeneratePdfAsync(Guid id);
        Task<PixPaymentDto> CreatePixPaymentAsync(Guid id);
        Task<PixPaymentDto?> GetPixPaymentAsync(Guid id);
        Task<PixPaymentDto> SyncPixPaymentAsync(Guid id);
        Task HandleMercadoPagoWebhookAsync(string externalPaymentId);
        Task<Guid> EnablePublicShareAsync(Guid id);
        Task<BudgetDto?> GetPublicByShareIdAsync(Guid shareId);
        Task<bool> PublicApproveAsync(Guid shareId, PublicBudgetDecisionDto dto);
        Task<bool> PublicRejectAsync(Guid shareId, PublicBudgetDecisionDto dto);
        Task<DashboardDto> GetDashboardStatsAsync();
    }
}
