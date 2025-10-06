using System.ComponentModel.DataAnnotations;

namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripEditDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Destination must be between 1 and 200 characters")]
        public string Destination { get; set; } = string.Empty;
    }
}
