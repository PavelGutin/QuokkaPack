using FluentAssertions;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class CategoriesTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly TestScope _scope;

        public CategoriesTests(ApiTestFactory factory)
        {
            _client = factory.CreateClient();
            _scope = TestScope.CreateAsync(factory).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/categories");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenCategoryExists()
        {
            var category = await SeedCategoryAsync();

            var response = await _client.GetAsync($"/api/categories/{category.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Category>();
            result.Should().NotBeNull();
            result!.Id.Should().Be(category.Id);
            result.Name.Should().Be(category.Name);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/categories/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAll_ShouldReturnCategories()
        {
            var category = await SeedCategoryAsync();

            var response = await _client.GetAsync("/api/categories");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var results = await response.Content.ReadFromJsonAsync<List<Category>>();
            results.Should().NotBeNull();
            results.Should().Contain(c => c.Id == category.Id);
        }

        [Fact]
        public async Task Post_ShouldReturnCreated_WhenValid()
        {
            var postData = new { Name = "Test", Description = "Testing", IsDefault = false };
            var response = await _client.PostAsJsonAsync("/api/categories", postData);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var putData = new { Id = 1234, Name = "Updated", Description = "Updated", IsDefault = true };
            var response = await _client.PutAsJsonAsync("/api/categories/9999", putData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnNoContent_WhenValid()
        {
            Category category = await SeedCategoryAsync();

            var putData = new { Id = category.Id, Name = "Updated", Description = "Updated Desc", IsDefault = true };
            var putResponse = await _client.PutAsJsonAsync($"/api/categories/{category.Id}", putData);

            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync("/api/categories/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            Category category = await SeedCategoryAsync();

            var deleteResponse = await _client.DeleteAsync($"/api/categories/{category.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        private async Task<Category> SeedCategoryAsync()
        {
            var category = BuildCategory(_scope.MasterUser.Id);
            _scope.Db.Categories.Add(category);
            await _scope.Db.SaveChangesAsync();
            return category;
        }

        private Category BuildCategory(Guid masterUserId)
        {
            return new Category
            {
                Name = "Test Category",
                MasterUserId = masterUserId,
                IsDefault = false
            };
        }
    }
}
