using QuokkaPack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuokkaPack.Shared.DTOs.Trip
{
    public class TripCreateDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
    }
}
