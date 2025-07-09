using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuokkaPack.ApiTests;
using Xunit;

namespace QuokkaPack.ApiTests.Controllers
{
    public class TripsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;

        public TripsTests(ApiTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/trips");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/trips/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldReturnCreated_WhenValid()
        {
            var postData = new { Name = "Test", Description = "Testing", IsDefault = false };
            var response = await _client.PostAsJsonAsync("/api/trips", postData);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var putData = new { Id = 1234, Name = "Updated", Description = "Updated", IsDefault = true };
            var response = await _client.PutAsJsonAsync("/api/trips/9999", putData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnNoContent_WhenValid()
        {
            int id = await TestSeedHelper.SeedCategoryAsync(_factory);

            var putData = new { Id = id, Name = "Updated", Description = "Updated Desc", IsDefault = true };
            var putResponse = await _client.PutAsJsonAsync($"/api/trips/{id}", putData);

            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync("/api/trips/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            int id = await TestSeedHelper.SeedCategoryAsync(_factory);

            var deleteResponse = await _client.DeleteAsync($"/api/trips/{id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
