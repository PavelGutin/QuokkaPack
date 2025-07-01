using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuokkaPack.Data.Models
{
    public class Trip
    {
        public int Id { get; set; }  // EF Core will make this the PK by convention
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Destination { get; set; } = string.Empty;
    }
}
