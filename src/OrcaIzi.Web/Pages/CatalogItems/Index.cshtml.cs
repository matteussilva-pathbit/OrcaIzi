﻿namespace OrcaIzi.Web.Pages.CatalogItems
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PagedResult<CatalogItemDto> CatalogItems { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _apiService.GetCatalogItemsAsync(PageNumber, PageSize, Search, false);
            if (result != null)
            {
                CatalogItems = result;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var success = await _apiService.DeleteCatalogItemAsync(id);
            if (!success)
            {
                TempData["Error"] = "Erro ao excluir item do catálogo.";
            }
            else
            {
                TempData["Success"] = "Item do catálogo excluído com sucesso.";
            }

            return RedirectToPage(new { PageNumber, Search });
        }
    }
}




