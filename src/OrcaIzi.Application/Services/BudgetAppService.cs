using OrcaIzi.Domain.Core;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using OrcaIzi.Application.Interfaces.Services;

namespace OrcaIzi.Application.Services
{
    public class BudgetAppService : IBudgetAppService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrcaIzi.Application.Interfaces.Services.IPdfService _pdfService;

        public BudgetAppService(
            IBudgetRepository budgetRepository, 
            ICustomerRepository customerRepository, 
            IHttpContextAccessor httpContextAccessor,
            OrcaIzi.Application.Interfaces.Services.IPdfService pdfService)
        {
            _budgetRepository = budgetRepository;
            _customerRepository = customerRepository;
            _httpContextAccessor = httpContextAccessor;
            _pdfService = pdfService;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<BudgetDto> CreateAsync(CreateBudgetDto budgetDto)
        {
            var userId = GetUserId();
            var customer = await _customerRepository.GetByIdAsync(budgetDto.CustomerId);
            if (customer == null || customer.UserId != userId) throw new Exception("Cliente não encontrado ou você não tem permissão.");

            var budget = new Budget(
                budgetDto.Title,
                budgetDto.Description,
                budgetDto.CustomerId,
                budgetDto.ExpirationDate,
                budgetDto.Observations
            );
            budget.SetOwner(userId);

            foreach (var item in budgetDto.Items)
            {
                budget.AddItem(item.Name, item.Description, item.Quantity, item.UnitPrice);
            }

            await _budgetRepository.AddAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            return MapToDto(budget);
        }

        public async Task<BudgetDto> UpdateAsync(Guid id, CreateBudgetDto budgetDto)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Orçamento não encontrado ou você não tem permissão.");

            var customer = await _customerRepository.GetByIdAsync(budgetDto.CustomerId);
            if (customer == null || customer.UserId != userId) throw new Exception("Cliente não encontrado ou você não tem permissão.");

            // Update main properties
            budget.Update(
                budgetDto.Title,
                budgetDto.Description,
                budgetDto.CustomerId,
                budgetDto.ExpirationDate,
                budgetDto.Observations
            );

            // Update Status
            if (Enum.TryParse<BudgetStatus>(budgetDto.Status, true, out var newStatus))
            {
                budget.UpdateStatus(newStatus);
            }

            // Update Items
            // Note: Since Budget.Items is read-only, we rely on the Budget methods.
            // ClearItems() removes them from the collection in memory.
            
            budget.ClearItems();
            foreach (var item in budgetDto.Items)
            {
                budget.AddItem(item.Name, item.Description, item.Quantity, item.UnitPrice);
            }

            // Important: We need to tell EF to update the Budget entity itself
            await _budgetRepository.UpdateWithItemsAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            // Force reload to ensure we return the latest state (especially if Customer changed)
            // and to avoid any tracking issues with the response mapping
            var updatedBudget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            return MapToDto(updatedBudget);
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Budget not found or you don't have permission");

            await _budgetRepository.DeleteAsync(id);
            await _budgetRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<BudgetDto>> GetAllAsync()
        {
            var userId = GetUserId();
            var budgets = await _budgetRepository.GetAllAsync(); // This gets all budgets
            return budgets.Where(b => b.UserId == userId).Select(MapToDto);
        }

        public async Task<PagedResult<BudgetDto>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var userId = GetUserId();
            var pagedBudgets = await _budgetRepository.GetAllPagedByUserIdAsync(userId, pageNumber, pageSize);
            
            return new PagedResult<BudgetDto>
            {
                Items = pagedBudgets.Items.Select(MapToDto),
                TotalCount = pagedBudgets.TotalCount,
                PageNumber = pagedBudgets.PageNumber,
                PageSize = pagedBudgets.PageSize
            };
        }

        public async Task<IEnumerable<BudgetDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var userId = GetUserId();
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null || customer.UserId != userId) return new List<BudgetDto>();

            var budgets = await _budgetRepository.GetByCustomerIdAsync(customerId);
            return budgets.Select(MapToDto);
        }

        public async Task<BudgetDto> GetByIdAsync(Guid id)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            if (budget == null || budget.UserId != userId) return null;

            return MapToDto(budget);
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Budget not found or you don't have permission");

            if (Enum.TryParse<BudgetStatus>(status, true, out var budgetStatus))
            {
                budget.UpdateStatus(budgetStatus);
                await _budgetRepository.UpdateAsync(budget);
                await _budgetRepository.SaveChangesAsync();
            }
        }

        private BudgetDto MapToDto(Budget budget)
        {
            return new BudgetDto
            {
                Id = budget.Id,
                Title = budget.Title,
                Description = budget.Description,
                CustomerId = budget.CustomerId,
                CustomerName = budget.Customer?.Name,
                TotalAmount = budget.TotalAmount,
                Status = budget.Status.ToString(),
                ExpirationDate = budget.ExpirationDate,
                Observations = budget.Observations,
                DigitalSignature = budget.DigitalSignature,
                Items = budget.Items.Select(i => new BudgetItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }

        public async Task<byte[]> GeneratePdfAsync(Guid id)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Budget not found or you don't have permission");

            var budgetDto = MapToDto(budget);
            return _pdfService.GenerateBudgetPdf(budgetDto);
        }

        public async Task<DashboardDto> GetDashboardStatsAsync()
        {
            var userId = GetUserId();
            var budgets = await _budgetRepository.GetAllAsync();
            var userBudgets = budgets.Where(b => b.UserId == userId).ToList();
            
            var customers = await _customerRepository.GetAllAsync();
            var userCustomersCount = customers.Count(c => c.UserId == userId);

            var stats = new DashboardDto
            {
                TotalBudgets = userBudgets.Count,
                PendingBudgets = userBudgets.Count(b => b.Status == BudgetStatus.Draft || b.Status == BudgetStatus.Sent),
                ApprovedBudgets = userBudgets.Count(b => b.Status == BudgetStatus.Approved),
                TotalCustomers = userCustomersCount,
                TotalRevenue = userBudgets.Where(b => b.Status == BudgetStatus.Approved).Sum(b => b.TotalAmount),
                BudgetsByStatus = userBudgets
                    .GroupBy(b => b.Status)
                    .Select(g => new BudgetStatusStatDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList()
            };

            return stats;
        }
    }
}
