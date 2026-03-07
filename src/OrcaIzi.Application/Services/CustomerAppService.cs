using OrcaIzi.Domain.Core;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace OrcaIzi.Application.Services
{
    public class CustomerAppService : ICustomerAppService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerAppService(ICustomerRepository customerRepository, IHttpContextAccessor httpContextAccessor)
        {
            _customerRepository = customerRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto customerDto)
        {
            var userId = GetUserId();
            var customer = new Customer(
                customerDto.Name,
                customerDto.Email,
                customerDto.Phone,
                customerDto.Document,
                customerDto.Address,
                customerDto.ProfilePictureUrl
            );
            customer.SetOwner(userId);

            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = GetUserId();
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null || customer.UserId != userId) throw new Exception("Customer not found or you don't have permission");

            await _customerRepository.DeleteAsync(id);
            await _customerRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var userId = GetUserId();
            var customers = await _customerRepository.GetByUserIdAsync(userId);
            return customers.Select(MapToDto);
        }

        public async Task<PagedResult<CustomerDto>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            var userId = GetUserId();
            var pagedCustomers = await _customerRepository.GetPagedByUserIdAsync(userId, pageNumber, pageSize);
            
            return new PagedResult<CustomerDto>
            {
                Items = pagedCustomers.Items.Select(MapToDto),
                TotalCount = pagedCustomers.TotalCount,
                PageNumber = pagedCustomers.PageNumber,
                PageSize = pagedCustomers.PageSize
            };
        }

        public async Task<CustomerDto> GetByIdAsync(Guid id)
        {
            var userId = GetUserId();
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null || customer.UserId != userId) return null;

            return MapToDto(customer);
        }

        public async Task<IEnumerable<CustomerDto>> SearchByNameAsync(string name)
        {
            var userId = GetUserId();
            var customers = await _customerRepository.SearchByNameAsync(name);
            return customers.Where(c => c.UserId == userId).Select(MapToDto);
        }

        public async Task UpdateAsync(Guid id, CreateCustomerDto customerDto)
        {
            var userId = GetUserId();
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null || customer.UserId != userId) throw new Exception("Customer not found or you don't have permission");

            customer.Update(
                customerDto.Name,
                customerDto.Email,
                customerDto.Phone,
                customerDto.Document,
                customerDto.Address,
                customerDto.ProfilePictureUrl
            );

            await _customerRepository.UpdateAsync(customer);
            await _customerRepository.SaveChangesAsync();
        }

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Document = customer.Document,
                Address = customer.Address
            };
        }
    }
}
