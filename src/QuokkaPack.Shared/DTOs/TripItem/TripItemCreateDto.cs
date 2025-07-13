using QuokkaPack.Shared.DTOs.ItemDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuokkaPack.Shared.DTOs.TripItem
{
    public class TripItemCreateDto
    {
        public int ItemId { get; set; } = default!;
        public bool IsPacked { get; set; } = false;
    }
}
