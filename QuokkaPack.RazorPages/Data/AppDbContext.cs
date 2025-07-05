using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<QuokkaPack.Shared.Models.Category> Category { get; set; } = default!;
    }
}
