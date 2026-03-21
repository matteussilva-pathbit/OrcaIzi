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
        public string? ZipCode { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Complement { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}


