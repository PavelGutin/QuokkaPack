using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuokkaPack.Shared.DTOs.ItemDTOs
{
    public class ItemReadDtoSimple
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }
}
