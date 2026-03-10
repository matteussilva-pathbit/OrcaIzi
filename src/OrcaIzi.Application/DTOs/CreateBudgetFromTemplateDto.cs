namespace OrcaIzi.Application.DTOs
{
    public class CreateBudgetFromTemplateDto
    {
        public Guid CustomerId { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Observations { get; set; }
    }
}
