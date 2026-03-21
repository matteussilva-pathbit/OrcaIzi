﻿namespace OrcaIzi.Domain.Entities
{
    public class BudgetItem : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice { get; private set; }
        public Guid BudgetId { get; private set; }
        public Budget Budget { get; private set; } = null!;

        protected BudgetItem() { } // EF Core

        public BudgetItem(string name, string? description, int quantity, decimal unitPrice, Guid budgetId)
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            BudgetId = budgetId;
            CalculateTotal();
        }

        public void CalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description, int quantity, decimal unitPrice)
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            CalculateTotal();
        }
    }
}


