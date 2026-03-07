using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;

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

        public BudgetDto? Budget { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Budget = await _apiService.GetBudgetByIdAsync(id);

            if (Budget == null)
            {
                return NotFound();
            }

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
    }
}
