namespace OrcaIzi.Domain.Entities
{
    public class CatalogItem : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? Unit { get; private set; }
        public string? Category { get; private set; }
        public decimal UnitPrice { get; private set; }
        public bool IsActive { get; private set; } = true;

        public string UserId { get; private set; } = string.Empty;
        public User User { get; private set; } = null!;

        protected CatalogItem() { }

        public CatalogItem(
            string name,
            decimal unitPrice,
            string? description = null,
            string? unit = null,
            string? category = null,
            bool isActive = true)
        {
            Name = name;
            UnitPrice = unitPrice;
            Description = description;
            Unit = unit;
            Category = category;
            IsActive = isActive;
        }

        public void Update(
            string name,
            decimal unitPrice,
            string? description,
            string? unit,
            string? category,
            bool isActive)
        {
            Name = name;
            UnitPrice = unitPrice;
            Description = description;
            Unit = unit;
            Category = category;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOwner(string userId)
        {
            UserId = userId;
        }
    }
}
