using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Domain.Core;
using OrcaIzi.Web.Interfaces;

namespace OrcaIzi.Web.Pages.Budgets
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
    }
}
