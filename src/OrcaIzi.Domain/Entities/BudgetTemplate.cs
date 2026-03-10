namespace OrcaIzi.Domain.Entities
{
    public class BudgetTemplate : BaseEntity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public string? Observations { get; private set; }

        public string UserId { get; private set; }
        public User User { get; private set; }

        private readonly List<BudgetTemplateItem> _items = new();
        public IReadOnlyCollection<BudgetTemplateItem> Items => _items.AsReadOnly();

        protected BudgetTemplate() { }

        public BudgetTemplate(string name, string? description, string? observations = null)
        {
            Name = name;
            Description = description;
            Observations = observations;
        }

        public void SetOwner(string userId)
        {
            UserId = userId;
        }

        public void Update(string name, string? description, string? observations = null)
        {
            Name = name;
            Description = description;
            Observations = observations;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddItem(string name, string? description, int quantity, decimal unitPrice)
        {
            var item = new BudgetTemplateItem(name, description, quantity, unitPrice, Id);
            _items.Add(item);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearItems()
        {
            _items.Clear();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
