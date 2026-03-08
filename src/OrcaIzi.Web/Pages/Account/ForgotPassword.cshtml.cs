using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;
using System.Threading.Tasks;

namespace OrcaIzi.Web.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IApiService _apiService;

        public ForgotPasswordModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public ForgotPasswordDto Input { get; set; }

        public bool Success { get; set; } = false;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _apiService.ForgotPasswordAsync(Input);

            if (result)
            {
                Success = true;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao processar sua solicitação.");
            }

            return Page();
        }
    }
}
