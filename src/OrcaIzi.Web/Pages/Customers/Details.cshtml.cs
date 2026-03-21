﻿namespace OrcaIzi.Web.Pages.Customers
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IApiService _apiService;

        public DetailsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public CustomerDto Customer { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var customer = await _apiService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            Customer = customer;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            await _apiService.DeleteCustomerAsync(id);
            TempData["Success"] = "Cliente excluído com sucesso!";
            return RedirectToPage("Index");
        }
    }
}



