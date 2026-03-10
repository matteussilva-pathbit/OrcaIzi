using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OrcaIzi.Application.DTOs;
using OrcaIzi.Domain.Entities;
using OrcaIzi.WebAPI.Controllers;
using Xunit;

namespace OrcaIzi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-for-testing-purposes-123");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("test-audience");
            _configurationMock.Setup(c => c["Jwt:ExpireDays"]).Returns("1");

            _loggerMock = new Mock<ILogger<AuthController>>();

            _controller = new AuthController(_userManagerMock.Object, _signInManagerMock.Object, _configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ZipCode = "12345678",
                Street = "Test St",
                Number = "123",
                Complement = "Apt 1",
                Neighborhood = "Test Neighborhood",
                City = "Test City",
                State = "TS"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var userDto = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(registerDto.Username, userDto.Username);
            Assert.Equal(registerDto.Email, userDto.Email);
            Assert.NotNull(userDto.Token);
        }

        [Fact]
        public async Task Register_ShouldReturn500_WhenJwtKeyIsMissing()
        {
             // Arrange
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns((string)null);
            
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}