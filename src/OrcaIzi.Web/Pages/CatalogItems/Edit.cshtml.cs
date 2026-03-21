﻿namespace OrcaIzi.Web.Pages.CatalogItems
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IApiService _apiService;

        public EditModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateCatalogItemDto Item { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var item = await _apiService.GetCatalogItemByIdAsync(id);
            if (item == null) return NotFound();

            Id = item.Id;
            Item = new CreateCatalogItemDto
            {
                Name = item.Name,
                Description = item.Description,
                Unit = item.Unit,
                Category = item.Category,
                UnitPrice = item.UnitPrice,
                IsActive = item.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var updated = await _apiService.UpdateCatalogItemAsync(Id, Item);
            if (updated == null)
            {
                ModelState.AddModelError(string.Empty, "Erro ao atualizar item do catálogo.");
                return Page();
            }

            TempData["Success"] = "Item do catálogo atualizado com sucesso!";
            return RedirectToPage("Index");
        }
    }
}




