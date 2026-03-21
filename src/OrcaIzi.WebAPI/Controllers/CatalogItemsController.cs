﻿namespace OrcaIzi.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogItemsController : ControllerBase
    {
        private readonly ICatalogItemAppService _catalogItemAppService;

        public CatalogItemsController(ICatalogItemAppService catalogItemAppService)
        {
            _catalogItemAppService = catalogItemAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool onlyActive = false)
        {
            var items = await _catalogItemAppService.GetAllPagedAsync(pageNumber, pageSize, search, onlyActive);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _catalogItemAppService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCatalogItemDto dto)
        {
            var created = await _catalogItemAppService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCatalogItemDto dto)
        {
            await _catalogItemAppService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _catalogItemAppService.DeleteAsync(id);
            return NoContent();
        }
    }
}




