using QuokkaPack.Shared.DTOs.ItemDTOs;

namespace QuokkaPack.Shared.DTOs.TripItem
{
    public class TripItemReadDto
    {
        public int Id { get; set; }
        public int TripId { get; set; } //TODO: Is this necessary? Maybe we can remove it later.
        public ItemReadDto ItemReadDto {get; set; } = default!;
        public bool IsPacked { get; set; } = false;

    }
}
