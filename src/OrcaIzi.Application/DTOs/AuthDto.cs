using System.ComponentModel.DataAnnotations;

namespace OrcaIzi.Application.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? Cnpj { get; set; }
        public string? CompanyAddress { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public string? Cnpj { get; set; }
        public string? CompanyAddress { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? CompanyLogoUrl { get; set; }
    }
}
