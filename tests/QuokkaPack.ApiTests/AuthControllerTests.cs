using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.Data;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class AuthControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;

        public AuthControllerTests(ApiTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // Note: AuthController tests are currently skipped because Identity/UserManager
        // requires SQL Server-specific configuration that doesn't work with InMemory database.
        // These tests would need a real database or extensive mocking to work properly.

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Register_ShouldReturnOk_WhenValidRequest()
        {
            var registerRequest = new
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                Password = "TestPassword123!",
                ConfirmPassword = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordsDoNotMatch()
        {
            var registerRequest = new
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                Password = "TestPassword123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Register_ShouldReturnBadRequest_WhenWeakPassword()
        {
            var registerRequest = new
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                Password = "weak",
                ConfirmPassword = "weak"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            var email = $"test{Guid.NewGuid()}@example.com";

            // First registration
            var registerRequest = new
            {
                Email = email,
                Password = "TestPassword123!",
                ConfirmPassword = "TestPassword123!"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Try to register again with same email
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Login_ShouldReturnOk_WhenValidCredentials()
        {
            // First, register a user
            var email = $"test{Guid.NewGuid()}@example.com";
            var password = "TestPassword123!";
            var registerRequest = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = password
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Now try to login
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Login_ShouldReturnUnauthorized_WhenInvalidPassword()
        {
            // First, register a user
            var email = $"test{Guid.NewGuid()}@example.com";
            var password = "TestPassword123!";
            var registerRequest = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = password
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Try to login with wrong password
            var loginRequest = new
            {
                Email = email,
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
        {
            var loginRequest = new
            {
                Email = $"nonexistent{Guid.NewGuid()}@example.com",
                Password = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(Skip = "Identity not configured for InMemory database in test environment")]
        public async Task Register_ShouldCreateMasterUser()
        {
            var email = $"test{Guid.NewGuid()}@example.com";
            var registerRequest = new
            {
                Email = email,
                Password = "TestPassword123!",
                ConfirmPassword = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify MasterUser was created
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var identityUser = await userManager.FindByEmailAsync(email);
            identityUser.Should().NotBeNull();

            var masterUser = context.MasterUsers
                .FirstOrDefault(mu => mu.IdentityUserId == identityUser!.Id);
            masterUser.Should().NotBeNull();
            masterUser!.Logins.Should().HaveCount(1);
            masterUser.Logins.First().Email.Should().Be(email);
        }

        private class TokenResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
