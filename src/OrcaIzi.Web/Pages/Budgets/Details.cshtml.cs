namespace OrcaIzi.Web.Pages.Budgets
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IApiService _apiService;

        public DetailsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public BudgetDto Budget { get; set; } = null!;
        public PixPaymentDto? Payment { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var budget = await _apiService.GetBudgetByIdAsync(id);
            if (budget == null) return NotFound();
            Budget = budget;

            Payment = await _apiService.GetBudgetPaymentAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var success = await _apiService.DeleteBudgetAsync(id);
            if (!success)
            {
                TempData["Error"] = "Erro ao excluir orçamento.";
                return Page();
            }

            TempData["Success"] = "Orçamento excluído com sucesso.";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostExportPdfAsync(Guid id)
        {
            var pdfBytes = await _apiService.GetBudgetPdfAsync(id);
            if (pdfBytes == null)
            {
                TempData["Error"] = "Erro ao gerar PDF.";
                return RedirectToPage(new { id });
            }

            return File(pdfBytes, "application/pdf", $"Orcamento_{id}.pdf");
        }

        public async Task<IActionResult> OnPostGeneratePixAsync(Guid id)
        {
            try
            {
                await _apiService.CreateBudgetPixPaymentAsync(id);
                TempData["Success"] = "Pix gerado. Envie o QR Code ou copie o código.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostSyncPaymentAsync(Guid id)
        {
            try
            {
                var payment = await _apiService.SyncBudgetPaymentAsync(id);
                if (payment != null && payment.Status.Equals("approved", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Success"] = "Pagamento confirmado. Status atualizado para Pago.";
                }
                else
                {
                    TempData["Success"] = "Pagamento sincronizado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostGeneratePublicLinkAsync(Guid id)
        {
            try
            {
                var shareId = await _apiService.EnableBudgetPublicShareAsync(id);
                if (shareId == null)
                {
                    TempData["Error"] = "Não foi possível gerar o link público.";
                    return RedirectToPage(new { id });
                }

                var url = $"{Request.Scheme}://{Request.Host}/Public/Budgets/{shareId}";
                TempData["Success"] = $"Link público gerado: {url}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostGeneratePublicLinkJsonAsync(Guid id)
        {
            try
            {
                var shareId = await _apiService.EnableBudgetPublicShareAsync(id);
                if (shareId == null)
                {
                    return new JsonResult(new { ok = false, message = "Não foi possível gerar o link público." });
                }

                var url = $"{Request.Scheme}://{Request.Host}/Public/Budgets/{shareId}";
                return new JsonResult(new { ok = true, url });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { ok = false, message = ex.Message });
            }
        }
    }
}



