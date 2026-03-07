namespace OrcaIzi.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Document { get; private set; } // CPF/CNPJ
        public string Address { get; private set; }
        public string? ProfilePictureUrl { get; private set; }

        public string UserId { get; private set; }
        public User User { get; private set; }

        public ICollection<Budget> Budgets { get; private set; } = new List<Budget>();

        protected Customer() { } // EF Core

        public Customer(string name, string email, string phone, string document = null, string address = null, string profilePictureUrl = null)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Document = document;
            Address = address;
            ProfilePictureUrl = profilePictureUrl;
        }

        public void Update(string name, string email, string phone, string document, string address, string profilePictureUrl = null)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Document = document;
            Address = address;
            if (profilePictureUrl != null) ProfilePictureUrl = profilePictureUrl;
            UpdatedAt = System.DateTime.UtcNow;
        }

        public void SetOwner(string userId)
        {
            UserId = userId;
        }
    }
}
