﻿namespace OrcaIzi.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetTemplatesController : ControllerBase
    {
        private readonly IBudgetTemplateAppService _templateAppService;

        public BudgetTemplatesController(IBudgetTemplateAppService templateAppService)
        {
            _templateAppService = templateAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var templates = await _templateAppService.GetAllPagedAsync(pageNumber, pageSize);
            return Ok(templates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var template = await _templateAppService.GetByIdAsync(id);
            if (template == null) return NotFound();
            return Ok(template);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetTemplateDto dto)
        {
            var template = await _templateAppService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CreateBudgetTemplateDto dto)
        {
            await _templateAppService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _templateAppService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/create-budget")]
        public async Task<IActionResult> CreateBudget(Guid id, CreateBudgetFromTemplateDto dto)
        {
            var budget = await _templateAppService.CreateBudgetFromTemplateAsync(id, dto);
            return CreatedAtAction("GetById", "Budgets", new { id = budget.Id }, budget);
        }
    }
}




