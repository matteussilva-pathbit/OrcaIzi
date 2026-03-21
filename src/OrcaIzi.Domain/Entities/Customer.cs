namespace OrcaIzi.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string Document { get; private set; } = string.Empty; // CPF/CNPJ
        public string Address { get; private set; } = string.Empty;
        public string? ProfilePictureUrl { get; private set; }

        public string UserId { get; private set; } = string.Empty;
        public User User { get; private set; } = null!;

        public ICollection<Budget> Budgets { get; private set; } = new List<Budget>();

        protected Customer() { } // EF Core

        public Customer(string name, string email, string phone, string? document = null, string? address = null, string? profilePictureUrl = null)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Document = document ?? string.Empty;
            Address = address ?? string.Empty;
            ProfilePictureUrl = profilePictureUrl;
        }

        public void Update(string name, string email, string phone, string? document, string? address, string? profilePictureUrl = null)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Document = document ?? string.Empty;
            Address = address ?? string.Empty;
            if (profilePictureUrl != null) ProfilePictureUrl = profilePictureUrl;
            UpdatedAt = System.DateTime.UtcNow;
        }

        public void SetOwner(string userId)
        {
            UserId = userId;
        }
    }
}
