using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Application.Interfaces;

namespace OrcaIzi.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IBudgetAppService _budgetAppService;
        private readonly IConfiguration _configuration;

        public PaymentsController(IBudgetAppService budgetAppService, IConfiguration configuration)
        {
            _budgetAppService = budgetAppService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("mercadopago/webhook")]
        public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoWebhookDto? payload)
        {
            var expectedToken = _configuration["Payments:WebhookToken"];
            if (!string.IsNullOrWhiteSpace(expectedToken))
            {
                var token = Request.Headers["X-Webhook-Token"].FirstOrDefault();
                if (!string.Equals(token, expectedToken, StringComparison.Ordinal))
                {
                    return Unauthorized();
                }
            }

            var externalId =
                payload?.Data?.Id
                ?? Request.Query["id"].FirstOrDefault()
                ?? Request.Query["data.id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(externalId))
            {
                return Ok();
            }

            await _budgetAppService.HandleMercadoPagoWebhookAsync(externalId);
            return Ok();
        }
    }
}
