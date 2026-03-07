using System;
using System.ComponentModel.DataAnnotations;

namespace OrcaIzi.Application.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }

    public class CreateCustomerDto
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
        public string? ProfilePictureUrl { get; set; }
    }
}
