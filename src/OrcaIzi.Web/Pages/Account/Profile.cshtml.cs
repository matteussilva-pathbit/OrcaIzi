using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OrcaIzi.Web.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly IWebHostEnvironment _environment;

        public ProfileModel(IApiService apiService, IWebHostEnvironment environment)
        {
            _apiService = apiService;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Display(Name = "Nome Completo")]
            public string? FullName { get; set; }

            [Display(Name = "Nome da Empresa")]
            public string? CompanyName { get; set; }

            [Display(Name = "CNPJ")]
            public string? Cnpj { get; set; }

            [Display(Name = "Endereço da Empresa")]
            public string? CompanyAddress { get; set; }

            [Display(Name = "Foto de Perfil")]
            public IFormFile? ProfilePicture { get; set; }
            public string? CurrentProfilePictureUrl { get; set; }

            [Display(Name = "Logo da Empresa")]
            public IFormFile? CompanyLogo { get; set; }
            public string? CurrentCompanyLogoUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _apiService.GetProfileAsync();
            if (user == null) return NotFound("Unable to load user profile.");

            Input = new InputModel
            {
                FullName = user.FullName,
                CompanyName = user.CompanyName,
                Cnpj = user.Cnpj,
                CompanyAddress = user.CompanyAddress,
                CurrentProfilePictureUrl = user.ProfilePictureUrl,
                CurrentCompanyLogoUrl = user.CompanyLogoUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // We need current profile to preserve existing URLs if not updated
            var currentUser = await _apiService.GetProfileAsync();
            if (currentUser == null) return NotFound("Unable to load user profile.");

            var updateDto = new UpdateProfileDto
            {
                FullName = Input.FullName,
                CompanyName = Input.CompanyName,
                Cnpj = Input.Cnpj,
                CompanyAddress = Input.CompanyAddress,
                ProfilePictureUrl = currentUser.ProfilePictureUrl,
                CompanyLogoUrl = currentUser.CompanyLogoUrl
            };

            // Handle Profile Picture Upload
            if (Input.ProfilePicture != null)
            {
                var fileName = $"profile_{Guid.NewGuid()}{Path.GetExtension(Input.ProfilePicture.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(stream);
                }
                updateDto.ProfilePictureUrl = $"/uploads/profiles/{fileName}";
            }

            // Handle Company Logo Upload
            if (Input.CompanyLogo != null)
            {
                var fileName = $"logo_{Guid.NewGuid()}{Path.GetExtension(Input.CompanyLogo.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.CompanyLogo.CopyToAsync(stream);
                }
                updateDto.CompanyLogoUrl = $"/uploads/logos/{fileName}";
            }

            var success = await _apiService.UpdateProfileAsync(updateDto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Erro ao atualizar perfil.");
                return Page();
            }

            TempData["Success"] = "Perfil atualizado com sucesso!";
            return RedirectToPage();
        }
    }
}