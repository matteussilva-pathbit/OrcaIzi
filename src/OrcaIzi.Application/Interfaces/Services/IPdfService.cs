using OrcaIzi.Application.DTOs;

namespace OrcaIzi.Application.Interfaces.Services
{
    public interface IPdfService
    {
        byte[] GenerateBudgetPdf(BudgetDto budget);
    }
}
