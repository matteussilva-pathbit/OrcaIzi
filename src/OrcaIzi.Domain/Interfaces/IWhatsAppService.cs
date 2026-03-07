using System.Threading.Tasks;
using OrcaIzi.Domain.Entities;

namespace OrcaIzi.Domain.Interfaces
{
    public interface IWhatsAppService
    {
        string GenerateWhatsAppLink(Budget budget);
    }
}
