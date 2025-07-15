using FluentAssertions;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.Trip;
using System.Net;
using System.Net.Http.Json;

namespace QuokkaPack.ApiTests.Controllers
{
    public class TripsTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        private readonly TestScope _scope;

        public TripsTests(ApiTestFactory factory)
        {
            _client = factory.CreateClient();
            _scope = TestScope.CreateAsync(factory).GetAwaiter().GetResult();
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
            var trip = new TripCreateDto() 
            { 
                CategoryIds = new List<int>(), 
                Destination = "Test Trip", 
                StartDate = DateOnly.FromDateTime(DateTime.Parse("2025/01/01")),
                EndDate = DateOnly.FromDateTime(DateTime.Parse("2025/02/01"))
            };
            var response = await _client.PostAsJsonAsync("/api/trips", trip);
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
            var trip = await SeedTripAsync();

            var tripEditDto = new TripEditDto 
            { 
                Id = trip.Id, 
                Destination = trip.Destination + " updated",
                StartDate = trip.StartDate.AddMonths(1),
                EndDate = trip.EndDate.AddMonths(1),
            };
            var putResponse = await _client.PutAsJsonAsync($"/api/trips/{trip.Id}", tripEditDto);

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
            var trip = await SeedTripAsync();

            var deleteResponse = await _client.DeleteAsync($"/api/trips/{trip.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        public async Task<Trip> SeedTripAsync()
        {
            var trip = CreateTrip(_scope.MasterUser.Id);
            _scope.Db.Trips.Add(trip);
            await _scope.Db.SaveChangesAsync();
            return trip;
        }
        private Trip CreateTrip(Guid masterUserId) => new Trip
        {
            Destination = "SeededTrip",
            StartDate = DateOnly.FromDateTime(DateTime.Parse("2025/01/01")),
            EndDate = DateOnly.FromDateTime(DateTime.Parse("2025/02/01")),
            MasterUserId = masterUserId
        };
    }
}
