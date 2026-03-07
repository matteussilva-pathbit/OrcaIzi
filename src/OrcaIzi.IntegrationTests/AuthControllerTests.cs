using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrcaIzi.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace OrcaIzi.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = $"test_{Guid.NewGuid()}@example.com",
                Username = $"user_{Guid.NewGuid()}",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<UserDto>();
            content.Should().NotBeNull();
            content.Email.Should().Be(registerDto.Email);
            content.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreCorrect()
        {
            // Arrange - Create user first
            var registerDto = new RegisterDto
            {
                Email = $"login_{Guid.NewGuid()}@example.com",
                Username = $"loginuser_{Guid.NewGuid()}",
                Password = "Password123!"
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Registration should succeed before login");

            var loginDto = new LoginDto
            {
                Username = registerDto.Username,
                Password = registerDto.Password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<UserDto>();
            content.Should().NotBeNull();
            content.Token.Should().NotBeNullOrEmpty();
        }
    }
}
