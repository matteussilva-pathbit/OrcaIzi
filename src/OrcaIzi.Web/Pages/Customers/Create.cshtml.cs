using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OrcaIzi.Web.Pages.Customers
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(IApiService apiService, IWebHostEnvironment environment)
        {
            _apiService = apiService;
            _environment = environment;
        }

        [BindProperty]
        public CreateCustomerDto Customer { get; set; } = new();

        [BindProperty]
        [Display(Name = "Foto do Cliente")]
        public IFormFile? ProfilePicture { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ProfilePicture != null)
            {
                var fileName = $"customer_{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "customers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }
                Customer.ProfilePictureUrl = $"/uploads/customers/{fileName}";
            }

            var createdCustomer = await _apiService.CreateCustomerAsync(Customer);
            if (createdCustomer == null)
            {
                ModelState.AddModelError(string.Empty, "Erro ao criar cliente.");
                return Page();
            }

            TempData["Success"] = "Cliente criado com sucesso!";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnGetConsultarCnpjAsync(string cnpj)
        {
            var result = await _apiService.ConsultarCnpjAsync(cnpj);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetConsultarCpfAsync(string cpf)
        {
            var result = await _apiService.ConsultarCpfAsync(cpf);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetConsultarCepAsync(string cep)
        {
            var result = await _apiService.ConsultarCepAsync(cep);
            return new JsonResult(result);
        }
    }
}
