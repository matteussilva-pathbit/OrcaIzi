namespace OrcaIzi.Application.DTOs
{
    public class BudgetTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Observations { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BudgetTemplateItemDto> Items { get; set; } = new();
    }

    public class BudgetTemplateItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
