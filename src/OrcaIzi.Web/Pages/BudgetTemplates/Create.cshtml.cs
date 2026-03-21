﻿namespace OrcaIzi.Web.Pages.BudgetTemplates
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
        public CreateBudgetTemplateDto Template { get; set; } = new();

        public Task<IActionResult> OnGetAsync()
        {
            Template.Items.Add(new CreateBudgetTemplateItemDto());
            return Task.FromResult<IActionResult>(Page());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _apiService.CreateBudgetTemplateAsync(Template);
                TempData["Success"] = "Template criado com sucesso!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}




