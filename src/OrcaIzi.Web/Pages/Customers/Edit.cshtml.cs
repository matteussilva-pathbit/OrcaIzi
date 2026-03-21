﻿namespace OrcaIzi.Web.Pages.Customers
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly IWebHostEnvironment _environment;

        public EditModel(IApiService apiService, IWebHostEnvironment environment)
        {
            _apiService = apiService;
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateCustomerInputModel Customer { get; set; } = new();

        public class CreateCustomerInputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "O telefone é obrigatório")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "O documento é obrigatório")]
            public string Document { get; set; } = string.Empty;

            public string Address { get; set; } = string.Empty;

            [Display(Name = "Foto do Cliente")]
            public IFormFile? ProfilePicture { get; set; }
            public string? CurrentProfilePictureUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var customer = await _apiService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            Id = customer.Id;
            Customer = new CreateCustomerInputModel
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Document = customer.Document,
                Address = customer.Address,
                CurrentProfilePictureUrl = customer.ProfilePictureUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var customerDto = new CreateCustomerDto
            {
                Name = Customer.Name,
                Email = Customer.Email,
                Phone = Customer.Phone,
                Document = Customer.Document,
                Address = Customer.Address,
                ProfilePictureUrl = Customer.CurrentProfilePictureUrl
            };

            if (Customer.ProfilePicture != null)
            {
                var fileName = $"customer_{Guid.NewGuid()}{Path.GetExtension(Customer.ProfilePicture.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "customers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Customer.ProfilePicture.CopyToAsync(stream);
                }
                customerDto.ProfilePictureUrl = $"/uploads/customers/{fileName}";
            }

            var updatedCustomer = await _apiService.UpdateCustomerAsync(Id, customerDto);
            if (updatedCustomer == null)
            {
                ModelState.AddModelError(string.Empty, "Erro ao atualizar cliente.");
                return Page();
            }

            TempData["Success"] = "Cliente atualizado com sucesso!";
            return RedirectToPage("Index");
        }
    }
}



