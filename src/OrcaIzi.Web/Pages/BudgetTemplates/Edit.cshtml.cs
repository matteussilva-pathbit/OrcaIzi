using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;

namespace OrcaIzi.Web.Pages.BudgetTemplates
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
        public CreateBudgetTemplateDto Template { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var template = await _apiService.GetBudgetTemplateByIdAsync(id);
            if (template == null) return NotFound();

            Id = template.Id;
            Template = new CreateBudgetTemplateDto
            {
                Name = template.Name,
                Description = template.Description,
                Observations = template.Observations,
                Items = template.Items.Select(i => new CreateBudgetTemplateItemDto
                {
                    Name = i.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            if (!Template.Items.Any())
            {
                Template.Items.Add(new CreateBudgetTemplateItemDto());
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _apiService.UpdateBudgetTemplateAsync(Id, Template);
                TempData["Success"] = "Template atualizado com sucesso!";
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

