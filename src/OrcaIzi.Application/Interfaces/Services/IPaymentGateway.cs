using OrcaIzi.Application.DTOs;
using OrcaIzi.Domain.Entities;

namespace OrcaIzi.Application.Interfaces.Services
{
    public interface IPaymentGateway
    {
        Task<PixPaymentDto> CreatePixPaymentAsync(Budget budget, Customer customer);
        Task<PixPaymentDto> GetPaymentAsync(string externalPaymentId);
    }
}
