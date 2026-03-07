using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;

namespace OrcaIzi.Web.Pages.Budgets
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
        public CreateBudgetDto Budget { get; set; } = new();

        public SelectList Customers { get; set; } = new SelectList(new List<CustomerDto>(), "Id", "Name");

        public async Task<IActionResult> OnGetAsync()
        {
            var customers = await _apiService.GetCustomersAsync(1, 100);
            Customers = new SelectList(customers?.Items ?? new List<CustomerDto>(), "Id", "Name");
            
            // Add initial empty item
            Budget.Items.Add(new CreateBudgetItemDto());
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload Customers
                var customers = await _apiService.GetCustomersAsync(1, 100);
                Customers = new SelectList(customers?.Items ?? new List<CustomerDto>(), "Id", "Name");
                return Page();
            }

            try
            {
                await _apiService.CreateBudgetAsync(Budget);
                TempData["Success"] = "Orçamento criado com sucesso!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // Reload Customers
                var customers = await _apiService.GetCustomersAsync(1, 100);
                Customers = new SelectList(customers?.Items ?? new List<CustomerDto>(), "Id", "Name");
                return Page();
            }
        }
    }
}
