﻿namespace OrcaIzi.Web.Pages.Budgets
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IApiService _apiService;

        public EditModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public CreateBudgetDto Budget { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public SelectList Customers { get; set; } = new SelectList(new List<CustomerDto>(), "Id", "Name");

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            // Load Customers for Dropdown
            var customers = await _apiService.GetCustomersAsync(1, 100);
            Customers = new SelectList(customers?.Items ?? new List<CustomerDto>(), "Id", "Name");

            var budget = await _apiService.GetBudgetByIdAsync(id);
            if (budget == null) return NotFound();

            Id = budget.Id;
            Budget = new CreateBudgetDto
            {
                Title = budget.Title,
                Description = budget.Description,
                CustomerId = budget.CustomerId,
                Status = budget.Status,
                ExpirationDate = budget.ExpirationDate,
                Items = budget.Items.Select(i => new CreateBudgetItemDto 
                { 
                    Name = i.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            return Page();
        }

        public async Task<IActionResult> OnGetCatalogItemsAsync(Guid id, string? q, int pageNumber = 1, int pageSize = 20)
        {
            var result = await _apiService.GetCatalogItemsAsync(pageNumber, pageSize, q, true);
            return new JsonResult(new { items = result?.Items ?? Enumerable.Empty<CatalogItemDto>() });
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
                var updatedBudget = await _apiService.UpdateBudgetAsync(Id, Budget);
                TempData["Success"] = "Orçamento atualizado com sucesso!";
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



