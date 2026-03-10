namespace OrcaIzi.Domain.Entities
{
    public class BudgetTemplateItem : BaseEntity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice { get; private set; }

        public Guid BudgetTemplateId { get; private set; }
        public BudgetTemplate BudgetTemplate { get; private set; }

        protected BudgetTemplateItem() { }

        public BudgetTemplateItem(string name, string? description, int quantity, decimal unitPrice, Guid budgetTemplateId)
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            BudgetTemplateId = budgetTemplateId;
            CalculateTotal();
        }

        public void Update(string name, string? description, int quantity, decimal unitPrice)
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            CalculateTotal();
        }

        public void CalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
