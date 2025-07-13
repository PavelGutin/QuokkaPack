using FluentAssertions;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;

//TODO clean up all the copy pasted code. I just need to get this working first
namespace QuokkaPack.ApiTests.Controllers
{
    public class CategoryItemsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly TestScope _scope;

        public CategoryItemsTests(ApiTestFactory factory)
        {
            _client = factory.CreateClient();
            _scope = TestScope.CreateAsync(factory).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var item = await SeedCategoryAndItemAsync();
            var response = await _client.GetAsync(BuildCategoryItemUrl(item.Categories.First().Id));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync(BuildCategoryItemUrl(999999));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldReturnCreated_WhenValid()
        {
            var item = await SeedCategoryAndItemAsync();

            var postData = new { categoryId = item.Categories.First().Id, itemId = item.Id };
            var response = await _client.PostAsJsonAsync(
                BuildCategoryItemUrl(item.Categories.First().Id, item.Id), postData);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            var response = await _client.DeleteAsync(BuildCategoryItemUrl(999999, 999999));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            var item = await SeedCategoryAndItemAsync();
            var deleteResponse = await _client.DeleteAsync(BuildCategoryItemUrl(item.Categories.First().Id, item.Id));
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        private static string BuildCategoryItemUrl(int categoryId, int itemId)
        {
            return $"api/categories/{categoryId}/items/{itemId}";
        }

        private static string BuildCategoryItemUrl(int categoryId)
        {
            return $"api/categories/{categoryId}/items/";
        }

        private async Task<Item> SeedCategoryAndItemAsync()
        {
            var category = CreateCategory(_scope.MasterUser.Id);
            var item = CreateItem(_scope.MasterUser.Id);
            item.Categories.Add(category);
            _scope.Db.Items.Add(item);
            await _scope.Db.SaveChangesAsync();
            return item;
        }

        private static Category CreateCategory(Guid masterUserId) => new Category
        {
            Name = "SeededCategory",
            Description = "Seeded for testing",
            IsDefault = false,
            MasterUserId = masterUserId
        };

        private static Item CreateItem(Guid masterUserId) => new Item
        {
            Name = "SeededItem",
            Notes = "Seeded for testing",
            IsEssential = false,
            MasterUserId = masterUserId
        };
    }
}
