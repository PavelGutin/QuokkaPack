using FluentAssertions;
using QuokkaPack.Shared.DTOs.Item;
using QuokkaPack.Shared.Mappings;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class ItemsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly TestScope _scope;

        public ItemsTests(ApiTestFactory factory)
        {
            _client = factory.CreateClient();
            _scope = TestScope.CreateAsync(factory).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/items");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/items/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldReturnCreated_WhenValid()
        {
            //var postData = new { Name = "Test", Description = "Testing", IsDefault = false };
            var itemCreateDto = new ItemCreateDto
            {
                Name = "Test Item",
                CategoryId = 1
            };
            var response = await _client.PostAsJsonAsync("/api/items", itemCreateDto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        {
            Item item = await SeedItemAsync();
            var itemDto = item.ToReadDto();
            var response = await _client.PutAsJsonAsync($"/api/items/{itemDto.Id + 1}", itemDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnNoContent_WhenValid()
        {
            Item item = await SeedItemAsync();
            var itemEditDto = new ItemEditDto()
            {
                Id = item.Id,
                Name = item.Name + " updated",
                CategoryId = item.CategoryId
            };
            var putResponse = await _client.PutAsJsonAsync($"/api/items/{item.Id}", itemEditDto);
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            Item item = await SeedItemAsync();
            var response = await _client.DeleteAsync($"/api/items/{item.Id + 1}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            var item = await SeedItemAsync();
            var deleteResponse = await _client.DeleteAsync($"/api/items/{item.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        public async Task<Item> SeedItemAsync()
        {
            var item = CreateItem(_scope.MasterUser.Id);
            _scope.Db.Items.Add(item);
            await _scope.Db.SaveChangesAsync();
            return item;
        }
        private Item CreateItem(Guid masterUserId) => new Item
        {
            Name = "SeededItem",
            MasterUserId = masterUserId
        };
    }
}
