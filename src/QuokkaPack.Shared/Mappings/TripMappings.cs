using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.Trip;

namespace QuokkaPack.Shared.Mappings
{
    public static class TripMappings
    {
        public static Trip ToTrip(this TripCreateDto dto)
        {
            return new Trip
            {
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Destination = dto.Destination
            };
        }
        public static TripReadDto ToReadDto(this Trip trip)
        {
            return new TripReadDto
            {
                Id = trip.Id,
                Destination = trip.Destination,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate
            };
        }
    }
}
