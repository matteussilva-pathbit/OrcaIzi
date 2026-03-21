﻿namespace OrcaIzi.Application.Services
{
    public class BudgetTemplateAppService : IBudgetTemplateAppService
    {
        private readonly IBudgetTemplateRepository _templateRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BudgetTemplateAppService(
            IBudgetTemplateRepository templateRepository,
            IBudgetRepository budgetRepository,
            ICustomerRepository customerRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _templateRepository = templateRepository;
            _budgetRepository = budgetRepository;
            _customerRepository = customerRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<BudgetTemplateDto?> GetByIdAsync(Guid id)
        {
            var userId = GetUserId();
            var template = await _templateRepository.GetWithItemsAsync(id);
            if (template == null || template.UserId != userId) return null;
            return MapToDto(template);
        }

        public async Task<PagedResult<BudgetTemplateDto>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var userId = GetUserId();
            var all = await _templateRepository.GetByUserIdAsync(userId);
            var totalCount = all.Count();
            var items = all
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return new PagedResult<BudgetTemplateDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BudgetTemplateDto> CreateAsync(CreateBudgetTemplateDto templateDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) throw new Exception("Usuário não autenticado ou ID inválido.");

            var template = new BudgetTemplate(templateDto.Name, templateDto.Description, templateDto.Observations);
            template.SetOwner(userId);

            foreach (var item in templateDto.Items)
            {
                template.AddItem(item.Name, item.Description, item.Quantity, item.UnitPrice);
            }

            await _templateRepository.AddAsync(template);
            await _templateRepository.SaveChangesAsync();

            var created = await _templateRepository.GetWithItemsAsync(template.Id);
            return MapToDto(created!);
        }

        public async Task UpdateAsync(Guid id, CreateBudgetTemplateDto templateDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) throw new Exception("Usuário não autenticado ou ID inválido.");

            var template = await _templateRepository.GetWithItemsAsync(id);
            if (template == null || template.UserId != userId) throw new Exception("Template não encontrado ou você não tem permissão.");

            template.Update(templateDto.Name, templateDto.Description, templateDto.Observations);
            template.ClearItems();
            foreach (var item in templateDto.Items)
            {
                template.AddItem(item.Name, item.Description, item.Quantity, item.UnitPrice);
            }

            await _templateRepository.UpdateWithItemsAsync(template);
            await _templateRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = GetUserId();
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null || template.UserId != userId) throw new Exception("Template não encontrado ou você não tem permissão.");

            await _templateRepository.DeleteAsync(id);
            await _templateRepository.SaveChangesAsync();
        }

        public async Task<BudgetDto> CreateBudgetFromTemplateAsync(Guid templateId, CreateBudgetFromTemplateDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) throw new Exception("Usuário não autenticado ou ID inválido.");

            var template = await _templateRepository.GetWithItemsAsync(templateId);
            if (template == null || template.UserId != userId) throw new Exception("Template não encontrado ou você não tem permissão.");

            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null || customer.UserId != userId) throw new Exception("Cliente não encontrado ou você não tem permissão.");

            var expiration = dto.ExpirationDate ?? DateTime.UtcNow.AddDays(7);

            var budget = new Budget(
                dto.Title ?? template.Name,
                dto.Description ?? template.Description ?? string.Empty,
                dto.CustomerId,
                expiration,
                dto.Observations ?? template.Observations
            );
            budget.SetOwner(userId);

            foreach (var item in template.Items)
            {
                budget.AddItem(item.Name, item.Description, item.Quantity, item.UnitPrice);
            }

            await _budgetRepository.AddAsync(budget);
            await _budgetRepository.SaveChangesAsync();

            var created = await _budgetRepository.GetWithItemsAndCustomerAsync(budget.Id);
            if (created == null) throw new Exception("Erro ao recarregar orçamento criado pelo template.");
            return MapBudgetToDto(created);
        }

        private BudgetTemplateDto MapToDto(BudgetTemplate template)
        {
            var items = template.Items.Select(i => new BudgetTemplateItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList();

            return new BudgetTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Observations = template.Observations,
                Items = items,
                TotalAmount = items.Sum(x => x.TotalPrice)
            };
        }

        private BudgetDto MapBudgetToDto(Budget budget)
        {
            var items = budget.Items.Select(i => new BudgetItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList();

            return new BudgetDto
            {
                Id = budget.Id,
                CreatedAt = budget.CreatedAt,
                Title = budget.Title,
                Description = budget.Description,
                CustomerId = budget.CustomerId,
                CustomerName = budget.Customer?.Name ?? string.Empty,
                CustomerEmail = budget.Customer?.Email,
                CustomerPhone = budget.Customer?.Phone,
                CustomerDocument = budget.Customer?.Document,
                CustomerAddress = budget.Customer?.Address,
                TotalAmount = budget.TotalAmount,
                Status = budget.Status.ToString(),
                ExpirationDate = budget.ExpirationDate,
                Observations = budget.Observations,
                DigitalSignature = budget.DigitalSignature,
                Items = items
            };
        }
    }
}



