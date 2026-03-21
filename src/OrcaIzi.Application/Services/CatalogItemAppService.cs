﻿namespace OrcaIzi.Application.Services
{
    public class CatalogItemAppService : ICatalogItemAppService
    {
        private readonly ICatalogItemRepository _catalogItemRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogItemAppService(ICatalogItemRepository catalogItemRepository, IHttpContextAccessor httpContextAccessor)
        {
            _catalogItemRepository = catalogItemRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new Exception("Usuário não autenticado ou ID inválido.");
            }

            return userId;
        }

        public async Task<CatalogItemDto> CreateAsync(CreateCatalogItemDto dto)
        {
            var userId = GetUserId();

            var item = new CatalogItem(
                dto.Name,
                dto.UnitPrice,
                dto.Description,
                dto.Unit,
                dto.Category,
                dto.IsActive);
            item.SetOwner(userId);

            await _catalogItemRepository.AddAsync(item);
            await _catalogItemRepository.SaveChangesAsync();

            return MapToDto(item);
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = GetUserId();
            var item = await _catalogItemRepository.GetByIdAsync(id);
            if (item == null || item.UserId != userId) throw new Exception("Catalog item not found or you don't have permission");

            await _catalogItemRepository.DeleteAsync(id);
            await _catalogItemRepository.SaveChangesAsync();
        }

        public async Task<PagedResult<CatalogItemDto>> GetAllPagedAsync(int pageNumber, int pageSize, string? search, bool onlyActive)
        {
            var userId = GetUserId();
            var paged = await _catalogItemRepository.GetPagedByUserIdAsync(userId, pageNumber, pageSize, search, onlyActive);

            return new PagedResult<CatalogItemDto>
            {
                Items = paged.Items.Select(MapToDto),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };
        }

        public async Task<CatalogItemDto?> GetByIdAsync(Guid id)
        {
            var userId = GetUserId();
            var item = await _catalogItemRepository.GetByIdAsync(id);
            if (item == null || item.UserId != userId) return null;

            return MapToDto(item);
        }

        public async Task UpdateAsync(Guid id, CreateCatalogItemDto dto)
        {
            var userId = GetUserId();
            var item = await _catalogItemRepository.GetByIdAsync(id);
            if (item == null || item.UserId != userId) throw new Exception("Catalog item not found or you don't have permission");

            item.Update(dto.Name, dto.UnitPrice, dto.Description, dto.Unit, dto.Category, dto.IsActive);

            await _catalogItemRepository.UpdateAsync(item);
            await _catalogItemRepository.SaveChangesAsync();
        }

        private static CatalogItemDto MapToDto(CatalogItem item)
        {
            return new CatalogItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Unit = item.Unit,
                Category = item.Category,
                UnitPrice = item.UnitPrice,
                IsActive = item.IsActive
            };
        }
    }
}




