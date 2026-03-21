﻿namespace OrcaIzi.Web.Pages.CatalogItems
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IApiService _apiService;

        public CreateModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public CreateCatalogItemDto Item { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var created = await _apiService.CreateCatalogItemAsync(Item);
            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "Erro ao criar item do catálogo.");
                return Page();
            }

            TempData["Success"] = "Item do catálogo criado com sucesso!";
            return RedirectToPage("Index");
        }
    }
}




