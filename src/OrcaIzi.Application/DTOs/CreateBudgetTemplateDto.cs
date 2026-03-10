namespace OrcaIzi.Application.DTOs
{
    public class CreateBudgetTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Observations { get; set; }
        public List<CreateBudgetTemplateItemDto> Items { get; set; } = new();
    }

    public class CreateBudgetTemplateItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
