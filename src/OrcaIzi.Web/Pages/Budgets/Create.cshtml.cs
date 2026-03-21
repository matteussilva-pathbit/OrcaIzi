﻿namespace OrcaIzi.Web.Pages.Budgets
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

        public async Task<IActionResult> OnGetCatalogItemsAsync(string? q, int pageNumber = 1, int pageSize = 20)
        {
            var result = await _apiService.GetCatalogItemsAsync(pageNumber, pageSize, q, true);
            return new JsonResult(new { items = result?.Items ?? Enumerable.Empty<CatalogItemDto>() });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug: Check received values
            // foreach (var item in Budget.Items)
            // {
            //     Console.WriteLine($"Item: {item.Name}, Price: {item.UnitPrice}");
            // }

            if (!ModelState.IsValid)
            {
                // Reload Customers
                var customers = await _apiService.GetCustomersAsync(1, 100);
                Customers = new SelectList(customers?.Items ?? new List<CustomerDto>(), "Id", "Name");
                return Page();
            }

            // Manually parse UnitPrice for each item if needed, but ModelBinder with pt-BR culture should handle it.
            // However, let's ensure the API receives correct decimals.
            // Since we are using an API Service, the DTO is serialized to JSON.
            // The issue might be the API Service not sending the correct JSON format for decimals or the API not parsing it correctly.
            // Let's verify the API Service implementation.

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



