﻿namespace OrcaIzi.WebAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/public/budgets")]
    public class PublicBudgetsController : ControllerBase
    {
        private readonly IBudgetAppService _budgetAppService;

        public PublicBudgetsController(IBudgetAppService budgetAppService)
        {
            _budgetAppService = budgetAppService;
        }

        [HttpGet("{shareId:guid}")]
        public async Task<IActionResult> GetByShareId(Guid shareId)
        {
            var budget = await _budgetAppService.GetPublicByShareIdAsync(shareId);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpPost("{shareId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid shareId, [FromBody] PublicBudgetDecisionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();
            var ok = await _budgetAppService.PublicApproveAsync(shareId, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{shareId:guid}/reject")]
        public async Task<IActionResult> Reject(Guid shareId, [FromBody] PublicBudgetDecisionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest();
            var ok = await _budgetAppService.PublicRejectAsync(shareId, dto);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}




