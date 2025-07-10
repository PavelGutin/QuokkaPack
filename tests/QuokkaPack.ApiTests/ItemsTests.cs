using FluentAssertions;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Mappings;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class ItemsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;

        public ItemsTests(ApiTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
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
                Notes = "Testing item creation",
                IsEssential = false
            };
            var response = await _client.PostAsJsonAsync("/api/items", itemCreateDto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        {
            Item item = await TestSeedHelper.SeedItemAsync(_factory);
            var itemDto = item.ToReadDto();
            var response = await _client.PutAsJsonAsync($"/api/items/{itemDto.Id + 1}", itemDto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnNoContent_WhenValid()
        {
            Item item = await TestSeedHelper.SeedItemAsync(_factory);
            var itemEditDto = new ItemEditDto() 
            { 
                Id = item.Id, 
                Name = item.Name + " updated", 
                Notes = item.Notes + " updated", 
                IsEssential = !item.IsEssential
            };
            var putResponse = await _client.PutAsJsonAsync($"/api/items/{item.Id}", itemEditDto);
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        {
            Item item = await TestSeedHelper.SeedItemAsync(_factory);
            var response = await _client.DeleteAsync($"/api/items/{item.Id + 1}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenValid()
        {
            var item = await TestSeedHelper.SeedItemAsync(_factory);
            var deleteResponse = await _client.DeleteAsync($"/api/items/{item.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
