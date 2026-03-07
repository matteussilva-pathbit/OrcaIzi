using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;
using OrcaIzi.Web.Services;

namespace OrcaIzi.Web.Pages.Customers
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IApiService _apiService;

        public DetailsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public CustomerDto? Customer { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Customer = await _apiService.GetCustomerByIdAsync(id);

            if (Customer == null)
            {
                return NotFound();
            }

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
