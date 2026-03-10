using OrcaIzi.Domain.Core;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using OrcaIzi.Application.Interfaces.Services;
using OrcaIzi.Domain.Interfaces;

namespace OrcaIzi.Application.Services
{
    public class BudgetAppService : IBudgetAppService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrcaIzi.Application.Interfaces.Services.IPdfService _pdfService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IWhatsAppService _whatsAppService;

        public BudgetAppService(
            IBudgetRepository budgetRepository, 
            ICustomerRepository customerRepository, 
            IHttpContextAccessor httpContextAccessor,
            OrcaIzi.Application.Interfaces.Services.IPdfService pdfService,
            IPaymentGateway paymentGateway,
            IWhatsAppService whatsAppService)
        {
            _budgetRepository = budgetRepository;
            _customerRepository = customerRepository;
            _httpContextAccessor = httpContextAccessor;
            _pdfService = pdfService;
            _paymentGateway = paymentGateway;
            _whatsAppService = whatsAppService;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<BudgetDto> CreateAsync(CreateBudgetDto budgetDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Usuário não autenticado ou ID inválido.");
            }

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

            try
            {
                await _budgetRepository.AddAsync(budget);
                await _budgetRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Erro ao salvar orçamento no banco de dados: {innerMessage}");
            }

            var created = await _budgetRepository.GetWithItemsAndCustomerAsync(budget.Id);
            return MapToDto(created);
        }

        public async Task<BudgetDto> UpdateAsync(Guid id, CreateBudgetDto budgetDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Usuário não autenticado ou ID inválido.");
            }

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

            try
            {
                // Important: We need to tell EF to update the Budget entity itself
                await _budgetRepository.UpdateWithItemsAsync(budget);
                await _budgetRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Erro ao atualizar orçamento no banco de dados: {innerMessage}");
            }

            // Force reload to ensure we return the latest state (especially if Customer changed)
            // and to avoid any tracking issues with the response mapping
            var updatedBudget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            return MapToDto(updatedBudget);
        }

        public async Task<BudgetDto> DuplicateAsync(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Usuário não autenticado ou ID inválido.");
            }

            var source = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            if (source == null || source.UserId != userId) throw new Exception("Orçamento não encontrado ou você não tem permissão.");

            var expirationDate = source.ExpirationDate > DateTime.Now ? source.ExpirationDate : DateTime.Now.AddDays(7);

            var duplicated = new Budget(
                $"{source.Title} (Cópia)",
                source.Description,
                source.CustomerId,
                expirationDate,
                source.Observations
            );
            duplicated.SetOwner(userId);

            foreach (var item in source.Items)
            {
                duplicated.AddItem(item.Name, item.Description, item.Quantity, item.UnitPrice);
            }

            try
            {
                await _budgetRepository.AddAsync(duplicated);
                await _budgetRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Erro ao duplicar orçamento no banco de dados: {innerMessage}");
            }

            var created = await _budgetRepository.GetWithItemsAndCustomerAsync(duplicated.Id);
            return MapToDto(created);
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Budget not found or you don't have permission");

            await _budgetRepository.DeleteAsync(id);
            await _budgetRepository.SaveChangesAsync();
        }

        public async Task<PixPaymentDto> CreatePixPaymentAsync(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Usuário não autenticado ou ID inválido.");
            }

            var budget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Orçamento não encontrado ou você não tem permissão.");
            if (budget.Customer == null) throw new Exception("Cliente não encontrado.");
            if (budget.TotalAmount <= 0) throw new Exception("Não é possível gerar Pix com valor total zero.");

            var payment = await _paymentGateway.CreatePixPaymentAsync(budget, budget.Customer);
            budget.SetPayment(payment.Provider, payment.ExternalId, payment.Status, payment.TicketUrl, payment.QrCode, payment.QrCodeBase64, payment.CreatedAt);

            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            return payment;
        }

        public async Task<PixPaymentDto?> GetPixPaymentAsync(Guid id)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null || budget.UserId != userId) return null;
            if (string.IsNullOrWhiteSpace(budget.PaymentExternalId)) return null;

            return new PixPaymentDto
            {
                Provider = budget.PaymentProvider ?? string.Empty,
                ExternalId = budget.PaymentExternalId ?? string.Empty,
                Status = budget.PaymentStatus ?? string.Empty,
                Amount = budget.TotalAmount,
                TicketUrl = budget.PaymentLink,
                QrCode = budget.PaymentQrCode,
                QrCodeBase64 = budget.PaymentQrCodeBase64,
                CreatedAt = budget.PaymentCreatedAt
            };
        }

        public async Task<PixPaymentDto> SyncPixPaymentAsync(Guid id)
        {
            var userId = GetUserId();
            var budget = await _budgetRepository.GetWithItemsAndCustomerAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Orçamento não encontrado ou você não tem permissão.");
            if (string.IsNullOrWhiteSpace(budget.PaymentExternalId)) throw new Exception("Este orçamento ainda não possui pagamento gerado.");

            var payment = await _paymentGateway.GetPaymentAsync(budget.PaymentExternalId);
            budget.UpdatePaymentStatus(payment.Status, IsPaidStatus(payment.Status) ? DateTime.UtcNow : null);
            if (IsPaidStatus(payment.Status))
            {
                budget.UpdateStatus(BudgetStatus.Paid);
            }

            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();
            return payment;
        }

        public async Task HandleMercadoPagoWebhookAsync(string externalPaymentId)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId)) return;

            var budget = await _budgetRepository.GetByPaymentExternalIdAsync(externalPaymentId);
            if (budget == null) return;

            var payment = await _paymentGateway.GetPaymentAsync(externalPaymentId);
            budget.UpdatePaymentStatus(payment.Status, IsPaidStatus(payment.Status) ? DateTime.UtcNow : null);
            if (IsPaidStatus(payment.Status))
            {
                budget.UpdateStatus(BudgetStatus.Paid);
            }

            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();
        }

        public async Task<Guid> EnablePublicShareAsync(Guid id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Usuário não autenticado ou ID inválido.");
            }

            var budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null || budget.UserId != userId) throw new Exception("Orçamento não encontrado ou você não tem permissão.");

            var shareId = budget.EnablePublicShare();
            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();
            return shareId;
        }

        public async Task<BudgetDto?> GetPublicByShareIdAsync(Guid shareId)
        {
            var budget = await _budgetRepository.GetByPublicShareIdAsync(shareId);
            if (budget == null) return null;
            return MapToDto(budget);
        }

        public async Task<bool> PublicApproveAsync(Guid shareId, PublicBudgetDecisionDto dto)
        {
            var budget = await _budgetRepository.GetByPublicShareIdAsync(shareId);
            if (budget == null) return false;

            budget.UpdateStatus(BudgetStatus.Approved);
            budget.SetDigitalSignature($"{dto.Name}|{dto.Document}|{DateTime.UtcNow:O}");
            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublicRejectAsync(Guid shareId, PublicBudgetDecisionDto dto)
        {
            var budget = await _budgetRepository.GetByPublicShareIdAsync(shareId);
            if (budget == null) return false;

            budget.UpdateStatus(BudgetStatus.Rejected);
            budget.SetDigitalSignature($"{dto.Name}|{dto.Document}|{DateTime.UtcNow:O}");
            await _budgetRepository.UpdateAsync(budget);
            await _budgetRepository.SaveChangesAsync();
            return true;
        }

        private static bool IsPaidStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return status.Equals("approved", StringComparison.OrdinalIgnoreCase)
                || status.Equals("accredited", StringComparison.OrdinalIgnoreCase)
                || status.Equals("paid", StringComparison.OrdinalIgnoreCase);
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
                CreatedAt = budget.CreatedAt,
                Title = budget.Title,
                Description = budget.Description,
                CustomerId = budget.CustomerId,
                CustomerName = budget.Customer?.Name,
                CustomerEmail = budget.Customer?.Email,
                CustomerPhone = budget.Customer?.Phone,
                CustomerDocument = budget.Customer?.Document,
                CustomerAddress = budget.Customer?.Address,
                CompanyName = budget.User?.CompanyName ?? budget.User?.FullName,
                CompanyLogoUrl = budget.User?.CompanyLogoUrl,
                CompanyCnpj = budget.User?.Cnpj,
                CompanyEmail = budget.User?.Email,
                CompanyPhone = budget.User?.PhoneNumber,
                CompanyAddress = budget.User?.CompanyAddress,
                TotalAmount = budget.TotalAmount,
                Status = budget.Status.ToString(),
                ExpirationDate = budget.ExpirationDate,
                Observations = budget.Observations,
                DigitalSignature = budget.DigitalSignature,
                WhatsAppLink = _whatsAppService.GenerateWhatsAppLink(budget),
                PaymentProvider = budget.PaymentProvider,
                PaymentExternalId = budget.PaymentExternalId,
                PaymentStatus = budget.PaymentStatus,
                PaymentLink = budget.PaymentLink,
                PaymentQrCode = budget.PaymentQrCode,
                PaymentQrCodeBase64 = budget.PaymentQrCodeBase64,
                PaymentCreatedAt = budget.PaymentCreatedAt,
                PaidAt = budget.PaidAt,
                PublicShareId = budget.PublicShareId,
                PublicShareEnabled = budget.PublicShareEnabled,
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
