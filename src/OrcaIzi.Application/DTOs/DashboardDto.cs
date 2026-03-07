namespace OrcaIzi.Application.DTOs
{
    public class DashboardDto
    {
        public int TotalBudgets { get; set; }
        public int PendingBudgets { get; set; }
        public int ApprovedBudgets { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; } // Sum of approved budgets maybe? Or all.
        public List<BudgetStatusStatDto> BudgetsByStatus { get; set; } = new();
    }

    public class BudgetStatusStatDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
