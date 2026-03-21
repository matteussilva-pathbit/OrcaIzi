namespace OrcaIzi.Application.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ZipCode { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Complement { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
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


