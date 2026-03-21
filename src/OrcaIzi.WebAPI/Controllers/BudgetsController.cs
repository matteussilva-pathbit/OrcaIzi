﻿namespace OrcaIzi.WebAPI.Controllers
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

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> Duplicate(Guid id)
        {
            try
            {
                var duplicated = await _budgetAppService.DuplicateAsync(id);
                return CreatedAtAction(nameof(GetById), new { id = duplicated.Id }, duplicated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
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

        [HttpGet("{id}/payment")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var payment = await _budgetAppService.GetPixPaymentAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpPost("{id}/payment/pix")]
        public async Task<IActionResult> CreatePixPayment(Guid id)
        {
            try
            {
                var payment = await _budgetAppService.CreatePixPaymentAsync(id);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("não configurado", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("AccessToken", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = ex.Message });
                }

                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{id}/payment/sync")]
        public async Task<IActionResult> SyncPayment(Guid id)
        {
            try
            {
                var payment = await _budgetAppService.SyncPixPaymentAsync(id);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("não configurado", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("AccessToken", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = ex.Message });
                }

                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> EnablePublicShare(Guid id)
        {
            try
            {
                var shareId = await _budgetAppService.EnablePublicShareAsync(id);
                return Ok(new { shareId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
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


