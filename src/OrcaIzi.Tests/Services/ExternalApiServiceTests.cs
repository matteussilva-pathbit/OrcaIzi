using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using OrcaIzi.Infrastructure.Services.External;
using System.Net;
using Xunit;

namespace OrcaIzi.Tests.Services
{
    public class ExternalApiServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockFactory;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ExternalApiService>> _mockLogger;
        private readonly ExternalApiService _service;

        public ExternalApiServiceTests()
        {
            _mockFactory = new Mock<IHttpClientFactory>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<ExternalApiService>>();
            
            // Mock HttpClient creation if needed, but validation logic doesn't use it immediately for invalid inputs
            var client = new HttpClient();
            _mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            // Mock cache
            object cacheEntry;
            _mockCache.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);
            _mockCache.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);

            _service = new ExternalApiService(_mockFactory.Object, _mockCache.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData("111.111.111-11", false)] // Invalid check digit (repeated)
        [InlineData("123.456.789-00", false)] // Invalid check digit
        [InlineData("52998224725", true)] // Valid CPF (generated)
        public async Task ConsultarCpfAsync_ShouldValidateCorrectly(string cpf, bool expectedValid)
        {
            var result = await _service.ConsultarCpfAsync(cpf);
            Assert.Equal(expectedValid, result.Valido);
        }

        [Fact]
        public async Task ConsultarCnpjAsync_ShouldReturnNull_WhenCnpjIsInvalid()
        {
            var result = await _service.ConsultarCnpjAsync("00.000.000/0000-00");
            Assert.Null(result);
        }
    }
}
