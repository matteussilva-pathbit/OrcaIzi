using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;

namespace OrcaIzi.Web.Pages.Public.Budgets
{
    public class DetailsModel : PageModel
    {
        private readonly IApiService _apiService;

        public DetailsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [FromRoute]
        public Guid ShareId { get; set; }

        public BudgetDto? Budget { get; set; }

        [BindProperty]
        public PublicBudgetDecisionDto Decision { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid shareId)
        {
            ShareId = shareId;
            Budget = await _apiService.GetPublicBudgetByShareIdAsync(shareId);
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid shareId)
        {
            ShareId = shareId;
            if (string.IsNullOrWhiteSpace(Decision.Name))
            {
                TempData["Error"] = "Informe seu nome.";
                return RedirectToPage(new { shareId });
            }

            var ok = await _apiService.PublicApproveBudgetAsync(shareId, Decision);
            TempData[ok ? "Success" : "Error"] = ok ? "Orçamento aprovado com sucesso." : "Não foi possível aprovar o orçamento.";
            return RedirectToPage(new { shareId });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid shareId)
        {
            ShareId = shareId;
            if (string.IsNullOrWhiteSpace(Decision.Name))
            {
                TempData["Error"] = "Informe seu nome.";
                return RedirectToPage(new { shareId });
            }

            var ok = await _apiService.PublicRejectBudgetAsync(shareId, Decision);
            TempData[ok ? "Success" : "Error"] = ok ? "Orçamento rejeitado." : "Não foi possível rejeitar o orçamento.";
            return RedirectToPage(new { shareId });
        }
    }
}

