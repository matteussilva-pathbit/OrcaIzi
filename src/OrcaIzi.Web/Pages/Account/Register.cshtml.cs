using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Web.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OrcaIzi.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IApiService _apiService;

        public RegisterModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "O usuário é obrigatório")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "A senha é obrigatória")]
            [DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "As senhas não conferem.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "O CEP é obrigatório")]
            public string ZipCode { get; set; } = string.Empty;

            public string? Street { get; set; }
            public string? Number { get; set; }
            public string? Complement { get; set; }
            public string? Neighborhood { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var registerDto = new RegisterDto
            {
                Email = Input.Email,
                Username = Input.Username,
                Password = Input.Password,
                ZipCode = Input.ZipCode,
                Street = Input.Street,
                Number = Input.Number,
                Complement = Input.Complement,
                Neighborhood = Input.Neighborhood,
                City = Input.City,
                State = Input.State
            };

            var user = await _apiService.RegisterAsync(registerDto);

            if (user != null && !string.IsNullOrEmpty(user.Token))
            {
                // Auto-login logic similar to Login page
                Response.Cookies.Append("AuthToken", user.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                });

                ViewData["AccountCreated"] = true;
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Falha ao registrar. Verifique os dados e tente novamente.");
            return Page();
        }
    }
}
