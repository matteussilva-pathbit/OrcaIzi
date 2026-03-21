namespace OrcaIzi.Domain.Entities
{
    public class Budget : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public Guid CustomerId { get; private set; }
        public Customer Customer { get; private set; } = null!;

        public string UserId { get; private set; } = string.Empty;
        public User User { get; private set; } = null!;
        public decimal TotalAmount { get; private set; }
        public BudgetStatus Status { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public string? Observations { get; private set; }
        public string? DigitalSignature { get; private set; } // Base64 or URL
        public string? PaymentProvider { get; private set; }
        public string? PaymentExternalId { get; private set; }
        public string? PaymentStatus { get; private set; }
        public string? PaymentLink { get; private set; }
        public string? PaymentQrCode { get; private set; }
        public string? PaymentQrCodeBase64 { get; private set; }
        public DateTime? PaymentCreatedAt { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public Guid? PublicShareId { get; private set; }
        public bool PublicShareEnabled { get; private set; }
        public DateTime? PublicShareCreatedAt { get; private set; }

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

        public void AddItem(string name, string? description, int quantity, decimal unitPrice)
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

        public void SetPayment(string provider, string externalId, string status, string? link, string? qrCode, string? qrCodeBase64, DateTime? createdAt)
        {
            PaymentProvider = provider;
            PaymentExternalId = externalId;
            PaymentStatus = status;
            PaymentLink = link;
            PaymentQrCode = qrCode;
            PaymentQrCodeBase64 = qrCodeBase64;
            PaymentCreatedAt = createdAt ?? DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePaymentStatus(string status, DateTime? paidAt = null)
        {
            PaymentStatus = status;
            if (paidAt != null)
            {
                PaidAt = paidAt;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public Guid EnablePublicShare()
        {
            if (PublicShareId == null)
            {
                PublicShareId = Guid.NewGuid();
            }

            PublicShareEnabled = true;
            PublicShareCreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            return PublicShareId.Value;
        }

        public void DisablePublicShare()
        {
            PublicShareEnabled = false;
            UpdatedAt = DateTime.UtcNow;
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
