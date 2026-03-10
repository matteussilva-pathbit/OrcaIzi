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
    public class BudgetTemplateAppServiceTests
    {
        private readonly Mock<IBudgetTemplateRepository> _templateRepositoryMock;
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly BudgetTemplateAppService _service;
        private readonly string _userId = "test-user-id";

        public BudgetTemplateAppServiceTests()
        {
            _templateRepositoryMock = new Mock<IBudgetTemplateRepository>();
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var context = new DefaultHttpContext();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId) };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _service = new BudgetTemplateAppService(
                _templateRepositoryMock.Object,
                _budgetRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTemplate_WhenValid()
        {
            BudgetTemplate? createdTemplate = null;

            _templateRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<BudgetTemplate>()))
                .Callback<BudgetTemplate>(t => createdTemplate = t)
                .Returns(Task.CompletedTask);

            _templateRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _templateRepositoryMock
                .Setup(x => x.GetWithItemsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() =>
                {
                    createdTemplate.Should().NotBeNull();
                    createdTemplate!.SetOwner(_userId);
                    return createdTemplate;
                });

            var dto = new CreateBudgetTemplateDto
            {
                Name = "Template 1",
                Description = "Desc",
                Items = new List<CreateBudgetTemplateItemDto>
                {
                    new CreateBudgetTemplateItemDto { Name = "Item", Description = null, Quantity = 2, UnitPrice = 10 }
                }
            };

            var result = await _service.CreateAsync(dto);

            result.Name.Should().Be("Template 1");
            result.Items.Should().HaveCount(1);
            result.Items[0].Description.Should().BeNull();
            result.TotalAmount.Should().Be(20);
            _templateRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BudgetTemplate>()), Times.Once);
            _templateRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBudgetFromTemplateAsync_ShouldCreateBudget_WhenCustomerBelongsToUser()
        {
            var templateId = Guid.NewGuid();
            var template = new BudgetTemplate("Template A", "D1");
            template.SetOwner(_userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(template, templateId);
            template.AddItem("Item 1", null, 1, 50);

            var customerId = Guid.NewGuid();
            var customer = new Customer("Customer", "c@test.com", "123", "doc", "addr");
            customer.SetOwner(_userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(customer, customerId);

            _templateRepositoryMock.Setup(x => x.GetWithItemsAsync(templateId)).ReturnsAsync(template);
            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId)).ReturnsAsync(customer);

            Budget? createdBudget = null;
            _budgetRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Budget>()))
                .Callback<Budget>(b => createdBudget = b)
                .Returns(Task.CompletedTask);
            _budgetRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _budgetRepositoryMock
                .Setup(x => x.GetWithItemsAndCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => createdBudget!);

            var budgetDto = await _service.CreateBudgetFromTemplateAsync(templateId, new CreateBudgetFromTemplateDto
            {
                CustomerId = customerId
            });

            budgetDto.Title.Should().Be("Template A");
            budgetDto.CustomerId.Should().Be(customerId);
            budgetDto.Items.Should().HaveCount(1);
            budgetDto.Items[0].Description.Should().BeNull();

            _budgetRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Budget>()), Times.Once);
            _budgetRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}

