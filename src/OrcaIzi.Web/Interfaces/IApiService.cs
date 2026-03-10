using OrcaIzi.Application.DTOs;
using OrcaIzi.Domain.Core;

namespace OrcaIzi.Web.Interfaces
{
    public interface IApiService
    {
        Task<UserDto?> LoginAsync(LoginDto loginDto);
        Task<UserDto?> RegisterAsync(RegisterDto registerDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<string?> GetTokenAsync();
        void Logout();

        // Budget Operations
        Task<PagedResult<BudgetDto>?> GetBudgetsAsync(int pageNumber, int pageSize);
        Task<BudgetDto?> GetBudgetByIdAsync(Guid id);
        Task<BudgetDto?> CreateBudgetAsync(CreateBudgetDto budgetDto);
        Task<BudgetDto?> UpdateBudgetAsync(Guid id, CreateBudgetDto budgetDto);
        Task<BudgetDto?> DuplicateBudgetAsync(Guid id);
        Task<bool> UpdateBudgetStatusAsync(Guid id, string status);
        Task<bool> DeleteBudgetAsync(Guid id);
        Task<byte[]?> GetBudgetPdfAsync(Guid id);
        Task<PixPaymentDto?> GetBudgetPaymentAsync(Guid id);
        Task<PixPaymentDto?> CreateBudgetPixPaymentAsync(Guid id);
        Task<PixPaymentDto?> SyncBudgetPaymentAsync(Guid id);
        Task<Guid?> EnableBudgetPublicShareAsync(Guid id);
        Task<BudgetDto?> GetPublicBudgetByShareIdAsync(Guid shareId);
        Task<bool> PublicApproveBudgetAsync(Guid shareId, PublicBudgetDecisionDto dto);
        Task<bool> PublicRejectBudgetAsync(Guid shareId, PublicBudgetDecisionDto dto);
        Task<DashboardDto?> GetDashboardStatsAsync();
        
        // Customer Operations
        Task<PagedResult<CustomerDto>?> GetCustomersAsync(int pageNumber, int pageSize);
        Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
        Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task<CustomerDto?> UpdateCustomerAsync(Guid id, CreateCustomerDto customerDto);
        Task<bool> DeleteCustomerAsync(Guid id);

        // Profile Operations
        Task<UserDto?> GetProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateProfileDto profileDto);

        // External Operations
        Task<CepResultDto?> ConsultarCepAsync(string cep);
        Task<CnpjResultDto?> ConsultarCnpjAsync(string cnpj);
        Task<CpfResultDto?> ConsultarCpfAsync(string cpf);

        // Budget Template Operations
        Task<PagedResult<BudgetTemplateDto>?> GetBudgetTemplatesAsync(int pageNumber, int pageSize);
        Task<BudgetTemplateDto?> GetBudgetTemplateByIdAsync(Guid id);
        Task<BudgetTemplateDto?> CreateBudgetTemplateAsync(CreateBudgetTemplateDto dto);
        Task<BudgetTemplateDto?> UpdateBudgetTemplateAsync(Guid id, CreateBudgetTemplateDto dto);
        Task<bool> DeleteBudgetTemplateAsync(Guid id);
        Task<BudgetDto?> CreateBudgetFromTemplateAsync(Guid templateId, CreateBudgetFromTemplateDto dto);
    }
}
