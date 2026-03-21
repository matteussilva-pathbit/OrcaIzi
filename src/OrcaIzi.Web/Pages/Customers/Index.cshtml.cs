﻿namespace OrcaIzi.Web.Pages.Customers
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PagedResult<CustomerDto> Customers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _apiService.GetCustomersAsync(PageNumber, PageSize);
            if (result != null)
            {
                Customers = result;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var success = await _apiService.DeleteCustomerAsync(id);
            if (!success)
            {
                TempData["Error"] = "Erro ao excluir cliente. Verifique se ele possui orçamentos vinculados.";
            }
            else
            {
                TempData["Success"] = "Cliente excluído com sucesso.";
            }

            return RedirectToPage();
        }
    }
}



