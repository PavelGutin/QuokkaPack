using QuokkaPack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuokkaPack.Shared.DTOs.ItemDTOs
{
    public class ItemReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Notes { get; set; }
        public bool IsEssential { get; set; } = false;
        public ICollection<Category> Categories { get; set; } = [];
    }
}
