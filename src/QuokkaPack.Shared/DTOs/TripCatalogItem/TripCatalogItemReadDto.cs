namespace QuokkaPack.Shared.DTOs.TripCatalogItem
{
    public enum ItemTripStatus { Packed, Unpacked, AvailableToAdd }

    public  class TripCatalogItemReadDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";

        public int? TripItemId { get; set; }
        public bool? IsPacked { get; set; }

        public ItemTripStatus Status =>
            TripItemId == null ? ItemTripStatus.AvailableToAdd :
            IsPacked == true ? ItemTripStatus.Packed :
            ItemTripStatus.Unpacked;
    }
}
