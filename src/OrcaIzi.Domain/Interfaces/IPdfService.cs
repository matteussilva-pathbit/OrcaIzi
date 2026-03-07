using System.Threading.Tasks;
using OrcaIzi.Domain.Entities;

namespace OrcaIzi.Domain.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateBudgetPdfAsync(Budget budget);
    }
}
