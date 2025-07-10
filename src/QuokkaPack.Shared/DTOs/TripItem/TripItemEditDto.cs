using QuokkaPack.Shared.DTOs.ItemDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuokkaPack.Shared.DTOs.TripItem
{
    public class TripItemEditDto
    {
        public int Id { get; set; }
        public int TripId { get; set; } //TODO: Is this necessary? Maybe we can remove it later.
        public ItemReadDto ItemReadDto { get; set; } = default!;
        public bool IsPacked { get; set; } = false;
    }
}
