namespace OrcaIzi.Domain.Entities
{
    public class Budget : BaseEntity
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public Guid CustomerId { get; private set; }
        public Customer Customer { get; private set; }

        public string UserId { get; private set; }
        public User User { get; private set; }
        public decimal TotalAmount { get; private set; }
        public BudgetStatus Status { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public string? Observations { get; private set; }
        public string? DigitalSignature { get; private set; } // Base64 or URL

        private readonly List<BudgetItem> _items = new List<BudgetItem>();
        public IReadOnlyCollection<BudgetItem> Items => _items.AsReadOnly();

        protected Budget() { } // EF Core

        public Budget(string title, string description, Guid customerId, DateTime expirationDate, string? observations = null)
        {
            Title = title;
            Description = description;
            CustomerId = customerId;
            ExpirationDate = expirationDate;
            Observations = observations;
            Status = BudgetStatus.Draft;
        }

        public void Update(string title, string description, Guid customerId, DateTime expirationDate, string? observations)
        {
            Title = title;
            Description = description;
            CustomerId = customerId;
            ExpirationDate = expirationDate;
            Observations = observations;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearItems()
        {
            _items.Clear();
            CalculateTotal();
        }

        public void AddItem(string name, string description, int quantity, decimal unitPrice)
        {
            var item = new BudgetItem(name, description, quantity, unitPrice, Id);
            _items.Add(item);
            CalculateTotal();
        }

        public void RemoveItem(Guid itemId)
        {
            var item = _items.FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                _items.Remove(item);
                CalculateTotal();
            }
        }

        public void CalculateTotal()
        {
            TotalAmount = _items.Sum(x => x.TotalPrice);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(BudgetStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDigitalSignature(string signature)
        {
            DigitalSignature = signature;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOwner(string userId)
        {
            UserId = userId;
        }
    }

    public enum BudgetStatus
    {
        Draft,
        Sent,
        Approved,
        Rejected,
        Cancelled,
        Paid
    }
}
