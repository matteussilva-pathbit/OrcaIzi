using Microsoft.AspNetCore.Authorization;

namespace OrcaIzi.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetAppService _budgetAppService;

        public BudgetsController(IBudgetAppService budgetAppService)
        {
            _budgetAppService = budgetAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var budgets = await _budgetAppService.GetAllPagedAsync(pageNumber, pageSize);
            return Ok(budgets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var budget = await _budgetAppService.GetByIdAsync(id);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetDto budgetDto)
        {
            var budget = await _budgetAppService.CreateAsync(budgetDto);
            return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateBudgetDto budgetDto)
        {
            try
            {
                var updatedBudget = await _budgetAppService.UpdateAsync(id, budgetDto);
                return Ok(updatedBudget);
            }
            catch (Exception ex)
            {
                // Return full error details for debugging
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            await _budgetAppService.UpdateStatusAsync(id, status);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _budgetAppService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("permission"))
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePdf(Guid id)
        {
            try
            {
                var pdfBytes = await _budgetAppService.GeneratePdfAsync(id);
                return File(pdfBytes, "application/pdf", $"Orcamento_{id}.pdf");
            }
            catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("permission"))
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _budgetAppService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}
