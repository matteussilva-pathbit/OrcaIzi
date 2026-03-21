namespace OrcaIzi.Tests.Application
{
    public class CatalogItemAppServiceTests
    {
        private readonly Mock<ICatalogItemRepository> _catalogItemRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly CatalogItemAppService _service;
        private readonly string _userId = "test-user-id";

        public CatalogItemAppServiceTests()
        {
            _catalogItemRepositoryMock = new Mock<ICatalogItemRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var context = new DefaultHttpContext();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId) };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _service = new CatalogItemAppService(_catalogItemRepositoryMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateCatalogItem_WhenValid()
        {
            CatalogItem? created = null;

            _catalogItemRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<CatalogItem>()))
                .Callback<CatalogItem>(i => created = i)
                .Returns(Task.CompletedTask);

            _catalogItemRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var dto = new CreateCatalogItemDto
            {
                Name = "Serviço A",
                Description = "Desc",
                Unit = "un",
                Category = "Mão de obra",
                UnitPrice = 49.90m,
                IsActive = true
            };

            var result = await _service.CreateAsync(dto);

            created.Should().NotBeNull();
            created!.UserId.Should().Be(_userId);
            result.Name.Should().Be("Serviço A");
            result.UnitPrice.Should().Be(49.90m);

            _catalogItemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CatalogItem>()), Times.Once);
            _catalogItemRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnMappedItems()
        {
            var item = new CatalogItem("Produto 1", 10m, null, "un", "Produtos", true);
            item.SetOwner(_userId);

            _catalogItemRepositoryMock
                .Setup(x => x.GetPagedByUserIdAsync(_userId, 1, 10, "prod", true))
                .ReturnsAsync(new PagedResult<CatalogItem>
                {
                    Items = new[] { item },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await _service.GetAllPagedAsync(1, 10, "prod", true);

            result.TotalCount.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Produto 1");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenItemNotOwned()
        {
            var item = new CatalogItem("X", 10m);
            item.SetOwner("other-user");
            var id = Guid.NewGuid();
            typeof(BaseEntity).GetProperty("Id")!.SetValue(item, id);

            _catalogItemRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(item);

            var act = async () => await _service.UpdateAsync(id, new CreateCatalogItemDto { Name = "Y", UnitPrice = 20m, IsActive = true });

            await act.Should().ThrowAsync<Exception>();
        }
    }
}



