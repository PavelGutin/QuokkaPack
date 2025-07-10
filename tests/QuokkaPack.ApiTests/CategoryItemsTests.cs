using FluentAssertions;
using QuokkaPack.ApiTests;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace QuokkaPack.ApiTests.Controllers
{
    public class CategoryItemsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;

        public CategoryItemsTests(ApiTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var (categoryId, itemId) = await SeedCategoryAndItemAsync();
            var response = await _client.GetAsync(BuildCategoryItemUrl(categoryId));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync(BuildCategoryItemUrl(999999, 999999));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldReturnCreated_WhenValid()
        {
            var (categoryId, itemId) = await SeedCategoryAndItemAsync();

            var postData = new { categoryId, itemId };
            var response = await _client.PostAsJsonAsync(
                BuildCategoryItemUrl(categoryId, itemId), postData);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }


        //[Fact]
        //public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        //{
        //    var putData = new { Id = 1234, Name = "Updated", Description = "Updated", IsDefault = true };
        //    var response = await _client.PutAsJsonAsync("/api/categoryitems/9999", putData);
        //    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task Put_ShouldReturnNoContent_WhenValid()
        //{
        //    var (categoryId, itemId) = await SeedCategoryAndItemAsync();

        //    var putData = new { Id = id, Name = "Updated", Description = "Updated Desc", IsDefault = true };
        //    var putResponse = await _client.PutAsJsonAsync($"/api/categoryitems/{id}", putData);

        //    putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        //}

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync("/api/categoryitems/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            var (categoryId, itemId) = await SeedCategoryAndItemAsync();

            var deleteResponse = await _client.DeleteAsync(BuildCategoryItemUrl(categoryId, itemId));
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        // Helper methods to seed data and build URLs
        private async Task<(int categoryId, int itemId)> SeedCategoryAndItemAsync()
        {
            var item = await TestSeedHelper.SeedCatgoryAndItemAsync(_factory);
            var categoryId = item.Categories.First().Id;
            var itemId = item.Id;
            return (categoryId, itemId);
        }

        private static string BuildCategoryItemUrl(int categoryId, int itemId)
        {
            return $"api/categories/{categoryId}/items/{itemId}";
        }

        private static string BuildCategoryItemUrl(int categoryId)
        {
            return $"api/categories/{categoryId}/items/";
        }
    }
}
