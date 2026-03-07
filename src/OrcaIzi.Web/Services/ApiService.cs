using OrcaIzi.Application.DTOs;
using OrcaIzi.Domain.Core;
using OrcaIzi.Web.Interfaces;
using System.Net.Http.Headers;

namespace OrcaIzi.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("Token")?.Value;
            
            // Fallback to cookie if user is not authenticated via Claims (e.g. during login process or if logic changes)
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
            }

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<UserDto?> LoginAsync(LoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user;
            }

            return null;
        }

        public async Task<UserDto?> RegisterAsync(RegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerDto);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user;
            }

            return null;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await Task.FromResult(_httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"]); 
        }

        public void Logout()
        {
            // Client-side logout logic handled in controller
        }

        public async Task<PagedResult<BudgetDto>?> GetBudgetsAsync(int pageNumber, int pageSize)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/Budgets?pageNumber={pageNumber}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PagedResult<BudgetDto>>();
            }
            return null;
        }

        public async Task<BudgetDto?> GetBudgetByIdAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/Budgets/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BudgetDto>();
            }
            return null;
        }

        public async Task<BudgetDto?> CreateBudgetAsync(CreateBudgetDto budgetDto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Budgets", budgetDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BudgetDto>();
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro ao criar orçamento ({response.StatusCode}): {errorContent}");
        }

        public async Task<BudgetDto?> UpdateBudgetAsync(Guid id, CreateBudgetDto budgetDto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/Budgets/{id}", budgetDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BudgetDto>();
            }
            
            // Read error content
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
        }

        public async Task<bool> UpdateBudgetStatusAsync(Guid id, string status)
        {
            AddAuthorizationHeader();
            // The controller expects [FromBody] string status, so we send it as JSON string
            var response = await _httpClient.PatchAsJsonAsync($"api/Budgets/{id}/status", status);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBudgetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/Budgets/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<byte[]?> GetBudgetPdfAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/Budgets/{id}/pdf");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }

        public async Task<DashboardDto?> GetDashboardStatsAsync()
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync("api/Budgets/dashboard");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DashboardDto>();
            }
            return null;
        }

        public async Task<PagedResult<CustomerDto>?> GetCustomersAsync(int pageNumber, int pageSize)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/Customers?pageNumber={pageNumber}&pageSize={pageSize}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PagedResult<CustomerDto>>();
            }
            return null;
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/Customers/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CustomerDto>();
            }
            return null;
        }

        public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto customerDto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Customers", customerDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CustomerDto>();
            }
            return null;
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, CreateCustomerDto customerDto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/Customers/{id}", customerDto);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new CustomerDto 
                    { 
                        Id = id, 
                        Name = customerDto.Name,
                        Email = customerDto.Email,
                        Phone = customerDto.Phone,
                        Document = customerDto.Document,
                        Address = customerDto.Address
                    };
                }

                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<CustomerDto>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    // Fallback if deserialization fails but status was success
                     return new CustomerDto 
                    { 
                        Id = id, 
                        Name = customerDto.Name,
                        Email = customerDto.Email,
                        Phone = customerDto.Phone,
                        Document = customerDto.Document,
                        Address = customerDto.Address
                    };
                }
            }
            return null;
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/Customers/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<UserDto?> GetProfileAsync()
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync("api/Auth/profile");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            return null;
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileDto profileDto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync("api/Auth/profile", profileDto);
            return response.IsSuccessStatusCode;
        }
    }
}
