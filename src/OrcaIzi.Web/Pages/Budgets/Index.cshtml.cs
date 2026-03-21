﻿namespace OrcaIzi.Web.Pages.Budgets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PagedResult<BudgetDto> Budgets { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _apiService.GetBudgetsAsync(PageNumber, PageSize);
            if (result != null)
            {
                Budgets = result;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var success = await _apiService.DeleteBudgetAsync(id);
            if (!success)
            {
                // Handle error (e.g., show message)
                TempData["Error"] = "Erro ao excluir orçamento.";
            }
            else
            {
                TempData["Success"] = "Orçamento excluído com sucesso.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportPdfAsync(Guid id)
        {
            var pdfBytes = await _apiService.GetBudgetPdfAsync(id);
            if (pdfBytes == null)
            {
                TempData["Error"] = "Erro ao gerar PDF.";
                return RedirectToPage();
            }

            return File(pdfBytes, "application/pdf", $"Orcamento_{id}.pdf");
        }

        public async Task<IActionResult> OnPostDuplicateAsync(Guid id)
        {
            try
            {
                var duplicated = await _apiService.DuplicateBudgetAsync(id);
                if (duplicated == null)
                {
                    TempData["Error"] = "Erro ao duplicar orçamento.";
                    return RedirectToPage();
                }

                TempData["Success"] = "Orçamento duplicado com sucesso.";
                return RedirectToPage("Edit", new { id = duplicated.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage();
            }
        }
    }
}



