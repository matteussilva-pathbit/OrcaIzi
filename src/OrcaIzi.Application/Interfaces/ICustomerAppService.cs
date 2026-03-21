namespace OrcaIzi.Application.Interfaces
{
    public interface ICustomerAppService
    {
        Task<CustomerDto> GetByIdAsync(Guid id);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<PagedResult<CustomerDto>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<CustomerDto> CreateAsync(CreateCustomerDto customerDto);
        Task UpdateAsync(Guid id, CreateCustomerDto customerDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<CustomerDto>> SearchByNameAsync(string name);
    }
}


