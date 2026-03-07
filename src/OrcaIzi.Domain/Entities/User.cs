using Microsoft.AspNetCore.Identity;

namespace OrcaIzi.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? Cnpj { get; set; }
        public string? CompanyAddress { get; set; }
    }
}
