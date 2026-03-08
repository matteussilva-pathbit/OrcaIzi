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
        Task<bool> UpdateBudgetStatusAsync(Guid id, string status);
        Task<bool> DeleteBudgetAsync(Guid id);
        Task<byte[]?> GetBudgetPdfAsync(Guid id);
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
    }
}
