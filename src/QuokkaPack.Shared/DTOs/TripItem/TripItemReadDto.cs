using QuokkaPack.Shared.DTOs.ItemDTOs;

namespace QuokkaPack.Shared.DTOs.TripItem
{
    public class TripItemReadDto
    {
        public int Id { get; set; }
        public ItemReadDto ItemReadDto {get; set; } = default!;
        public bool IsPacked { get; set; } = false;

    }
}
