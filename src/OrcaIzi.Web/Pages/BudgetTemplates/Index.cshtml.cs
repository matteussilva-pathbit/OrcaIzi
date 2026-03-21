﻿namespace OrcaIzi.Web.Pages.BudgetTemplates
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PagedResult<BudgetTemplateDto> Templates { get; set; } = new();
        public SelectList Customers { get; set; } = new SelectList(new List<CustomerDto>(), "Id", "Name");

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [BindProperty]
        public Guid SelectedCustomerId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _apiService.GetBudgetTemplatesAsync(PageNumber, PageSize);
            if (result != null)
            {
                Templates = result;
            }

            var customers = await _apiService.GetCustomersAsync(1, 100);
            Customers = new SelectList(customers?.Items ?? new List<CustomerDto>(), "Id", "Name");
            SelectedCustomerId = customers?.Items?.FirstOrDefault()?.Id ?? Guid.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var success = await _apiService.DeleteBudgetTemplateAsync(id);
            if (!success)
            {
                TempData["Error"] = "Erro ao excluir template.";
            }
            else
            {
                TempData["Success"] = "Template excluído com sucesso.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateBudgetAsync(Guid id)
        {
            if (SelectedCustomerId == Guid.Empty)
            {
                TempData["Error"] = "Selecione um cliente para criar o orçamento.";
                return RedirectToPage();
            }

            try
            {
                var budget = await _apiService.CreateBudgetFromTemplateAsync(id, new CreateBudgetFromTemplateDto
                {
                    CustomerId = SelectedCustomerId
                });

                if (budget == null)
                {
                    TempData["Error"] = "Erro ao criar orçamento a partir do template.";
                    return RedirectToPage();
                }

                TempData["Success"] = "Orçamento criado a partir do template.";
                return RedirectToPage("/Budgets/Edit", new { id = budget.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage();
            }
        }
    }
}




