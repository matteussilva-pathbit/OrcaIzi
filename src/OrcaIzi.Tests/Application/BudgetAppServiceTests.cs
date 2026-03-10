using FluentAssertions;
using Moq;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Application.Services;
using OrcaIzi.Domain.Entities;
using OrcaIzi.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;

namespace OrcaIzi.Tests.Application
{
    public class BudgetAppServiceTests
    {
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<OrcaIzi.Application.Interfaces.Services.IPdfService> _pdfServiceMock;
        private readonly Mock<OrcaIzi.Application.Interfaces.Services.IPaymentGateway> _paymentGatewayMock;
        private readonly Mock<IWhatsAppService> _whatsAppServiceMock;
        private readonly BudgetAppService _budgetAppService;
        private readonly string _userId = "test-user-id";

        public BudgetAppServiceTests()
        {
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _pdfServiceMock = new Mock<OrcaIzi.Application.Interfaces.Services.IPdfService>();
            _paymentGatewayMock = new Mock<OrcaIzi.Application.Interfaces.Services.IPaymentGateway>();
            _whatsAppServiceMock = new Mock<IWhatsAppService>();

            var context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            context.User = new ClaimsPrincipal(identity);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _budgetAppService = new BudgetAppService(
                _budgetRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _pdfServiceMock.Object,
                _paymentGatewayMock.Object,
                _whatsAppServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateBudget_WhenCustomerExistsAndBelongsToUser()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer("Test Customer", "test@test.com", "123456789", "123", "Address");
            customer.SetOwner(_userId);
            // Simulate EF Core setting ID
            typeof(BaseEntity).GetProperty("Id").SetValue(customer, customerId);

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            Budget? createdBudget = null;
            _budgetRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Budget>()))
                .Callback<Budget>(b =>
                {
                    createdBudget = b;
                    typeof(Budget).GetProperty("Customer")!.SetValue(createdBudget, customer);
                })
                .Returns(Task.CompletedTask);

            _budgetRepositoryMock
                .Setup(x => x.GetWithItemsAndCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => createdBudget!);

            var createDto = new CreateBudgetDto
            {
                Title = "New Budget",
                CustomerId = customerId,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                Items = new List<CreateBudgetItemDto>
                {
                    new CreateBudgetItemDto { Name = "Item 1", Quantity = 1, UnitPrice = 100 }
                }
            };

            // Act
            var result = await _budgetAppService.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(createDto.Title);
            result.TotalAmount.Should().Be(100);
            _budgetRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Budget>()), Times.Once);
            _budgetRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCustomerDoesNotBelongToUser()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer("Other Customer", "other@test.com", "987654321", "321", "Address");
            customer.SetOwner("other-user-id"); // Different user
            
            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            var createDto = new CreateBudgetDto
            {
                Title = "Budget",
                CustomerId = customerId
            };

            // Act
            Func<Task> act = async () => await _budgetAppService.CreateAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
            .WithMessage("Cliente não encontrado ou você não tem permissão.");
        }
    }
}
