﻿using QuokkaPack.Shared.DTOs.ItemDTOs;
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
        public bool IsPacked { get; set; } = false;
    }
}
