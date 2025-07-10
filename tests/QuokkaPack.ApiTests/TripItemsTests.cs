using FluentAssertions;
using QuokkaPack.ApiTests;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.DTOs.TripItem;
using QuokkaPack.Shared.Mappings;
using QuokkaPack.Shared.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace QuokkaPack.ApiTests.Controllers
{
    public class TripItemsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestFactory _factory;

        public TripItemsTests(ApiTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var (tripId, tripItemId) = await SeedTripAndItemAsync();
            var response = await _client.GetAsync(BuildTripItemUrl(tripId));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        //[Fact]
        //public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
        //{
        //    var response = await _client.GetAsync(BuildTripItemUrl(999999));
        //    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        //}

        //[Fact]
        //public async Task Post_ShouldReturnCreated_WhenValid()
        //{
        //    var (tripId, item) = await SeedTripAndItemAsync();

        //    var tripItemCreateDto = new TripItemCreateDto
        //    {
        //        TripId = tripId,
        //        ItemReadDto = item.ToReadDto(),
        //        IsPacked = false
        //    };

        //    var response = await _client.PostAsJsonAsync(BuildTripItemUrl(tripId), tripItemCreateDto);
        //    response.StatusCode.Should().Be(HttpStatusCode.Created);
        //}

        //[Fact]
        //public async Task Put_ShouldReturnBadRequest_WhenIdMismatch()
        //{
        //    var putData = new { Id = 1234, Name = "Updated", Description = "Updated", IsDefault = true };
        //    var response = await _client.PutAsJsonAsync("/api/tripitems/9999", putData);
        //    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task Put_ShouldReturnNoContent_WhenValid()
        //{
        //    var (tripId, item) = await SeedTripAndItemAsync();

        //    var tripItemEditDto = new TripItemEditDto
        //    {
        //        TripId = tripId,
        //        ItemReadDto = item.ToReadDto(),
        //        IsPacked = false
        //    };
        //    var putResponse = await _client.PutAsJsonAsync(BuildTripItemUrl(tripId), tripItemEditDto);

        //    putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        //}

        //[Fact]
        //public async Task Delete_ShouldReturnNotFound_WhenIdInvalid()
        //{
        //    var response = await _client.DeleteAsync(BuildTripItemUrl(999999, 999999));
        //    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        //}

        //[Fact]
        //public async Task Delete_ShouldReturnNoContent_WhenValid()
        //{
        //    var (tripId, tripItemId) = await SeedTripAndItemAsync();
        //    var deleteResponse = await _client.DeleteAsync(BuildTripItemUrl(tripId, tripItemId));
        //    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        //}

        // Helper methods to seed data and build URLs

        private async Task<(int tripId, Item item)> SeedTripAndItemAsync()
        {
            var trip = await TestSeedHelper.SeedTripAsync(_factory);
            var item = await TestSeedHelper.SeedItemAsync(_factory);
            return (trip.Id, item);
        }

        private static string BuildTripItemUrl(int tripId, int itemId)
        {
            return $"api/trips/{tripId}/items/{itemId}";
        }

        private static string BuildTripItemUrl(int tripId)
        {
            return $"api/trips/{tripId}/items";
        }
    }
}
