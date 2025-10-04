using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.Models;

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

        public static void UpdateFromDto(this Trip trip, TripEditDto dto)
        {
            trip.Destination = dto.Destination;
            trip.StartDate = dto.StartDate;
            trip.EndDate = dto.EndDate;
        }
    }
}
