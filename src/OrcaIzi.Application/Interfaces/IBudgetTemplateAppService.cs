﻿namespace OrcaIzi.Application.Interfaces
{
    public interface IBudgetTemplateAppService
    {
        Task<BudgetTemplateDto?> GetByIdAsync(Guid id);
        Task<PagedResult<BudgetTemplateDto>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<BudgetTemplateDto> CreateAsync(CreateBudgetTemplateDto templateDto);
        Task UpdateAsync(Guid id, CreateBudgetTemplateDto templateDto);
        Task DeleteAsync(Guid id);
        Task<BudgetDto> CreateBudgetFromTemplateAsync(Guid templateId, CreateBudgetFromTemplateDto dto);
    }
}


