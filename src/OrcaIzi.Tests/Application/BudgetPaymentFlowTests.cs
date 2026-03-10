using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Application.Services;
using OrcaIzi.Domain.Entities;
using OrcaIzi.Domain.Interfaces;
using System.Security.Claims;
using Xunit;

namespace OrcaIzi.Tests.Application
{
    public class BudgetPaymentFlowTests
    {
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<OrcaIzi.Application.Interfaces.Services.IPdfService> _pdfServiceMock;
        private readonly Mock<OrcaIzi.Application.Interfaces.Services.IPaymentGateway> _paymentGatewayMock;
        private readonly Mock<IWhatsAppService> _whatsAppServiceMock;
        private readonly BudgetAppService _service;
        private readonly string _userId = "test-user-id";

        public BudgetPaymentFlowTests()
        {
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _pdfServiceMock = new Mock<OrcaIzi.Application.Interfaces.Services.IPdfService>();
            _paymentGatewayMock = new Mock<OrcaIzi.Application.Interfaces.Services.IPaymentGateway>();
            _whatsAppServiceMock = new Mock<IWhatsAppService>();

            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId)
            }, "TestAuthType"));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _service = new BudgetAppService(
                _budgetRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _pdfServiceMock.Object,
                _paymentGatewayMock.Object,
                _whatsAppServiceMock.Object
            );
        }

        [Fact]
        public async Task CreatePixPaymentAsync_ShouldPersistPaymentData()
        {
            var budgetId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var customer = new Customer("Customer", "c@test.com", "123", "doc", "addr");
            customer.SetOwner(_userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(customer, customerId);

            var budget = new Budget("Budget", "Desc", customerId, DateTime.UtcNow.AddDays(7));
            budget.SetOwner(_userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(budget, budgetId);
            budget.AddItem("Item", null, 1, 50);
            typeof(Budget).GetProperty("Customer")!.SetValue(budget, customer);

            _budgetRepositoryMock.Setup(x => x.GetWithItemsAndCustomerAsync(budgetId)).ReturnsAsync(budget);

            var payment = new PixPaymentDto
            {
                Provider = "MercadoPago",
                ExternalId = "123",
                Status = "pending",
                Amount = 50,
                QrCode = "code",
                QrCodeBase64 = "base64",
                TicketUrl = "https://ticket",
                CreatedAt = DateTime.UtcNow
            };

            _paymentGatewayMock.Setup(x => x.CreatePixPaymentAsync(budget, customer)).ReturnsAsync(payment);

            Budget? updatedBudget = null;
            _budgetRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Budget>()))
                .Callback<Budget>(b => updatedBudget = b)
                .Returns(Task.CompletedTask);
            _budgetRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.CreatePixPaymentAsync(budgetId);

            result.ExternalId.Should().Be("123");
            updatedBudget.Should().NotBeNull();
            updatedBudget!.PaymentExternalId.Should().Be("123");
            updatedBudget.PaymentStatus.Should().Be("pending");
            updatedBudget.PaymentQrCode.Should().Be("code");
            updatedBudget.PaymentQrCodeBase64.Should().Be("base64");
            updatedBudget.PaymentLink.Should().Be("https://ticket");
        }

        [Fact]
        public async Task SyncPixPaymentAsync_ShouldMarkBudgetAsPaid_WhenApproved()
        {
            var budgetId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var customer = new Customer("Customer", "c@test.com", "123", "doc", "addr");
            customer.SetOwner(_userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(customer, customerId);

            var budget = new Budget("Budget", "Desc", customerId, DateTime.UtcNow.AddDays(7));
            budget.SetOwner(_userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(budget, budgetId);
            budget.AddItem("Item", null, 1, 50);
            typeof(Budget).GetProperty("Customer")!.SetValue(budget, customer);
            budget.SetPayment("MercadoPago", "123", "pending", "link", "code", "base64", DateTime.UtcNow);

            _budgetRepositoryMock.Setup(x => x.GetWithItemsAndCustomerAsync(budgetId)).ReturnsAsync(budget);

            _paymentGatewayMock.Setup(x => x.GetPaymentAsync("123")).ReturnsAsync(new PixPaymentDto
            {
                Provider = "MercadoPago",
                ExternalId = "123",
                Status = "approved",
                Amount = 50
            });

            _budgetRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Budget>())).Returns(Task.CompletedTask);
            _budgetRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.SyncPixPaymentAsync(budgetId);

            result.Status.Should().Be("approved");
            budget.Status.Should().Be(BudgetStatus.Paid);
            budget.PaidAt.Should().NotBeNull();
        }
    }
}

