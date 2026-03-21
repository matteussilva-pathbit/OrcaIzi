﻿namespace OrcaIzi.Application.Interfaces
{
    public interface ICatalogItemAppService
    {
        Task<CatalogItemDto?> GetByIdAsync(Guid id);
        Task<PagedResult<CatalogItemDto>> GetAllPagedAsync(int pageNumber, int pageSize, string? search, bool onlyActive);
        Task<CatalogItemDto> CreateAsync(CreateCatalogItemDto dto);
        Task UpdateAsync(Guid id, CreateCatalogItemDto dto);
        Task DeleteAsync(Guid id);
    }
}



