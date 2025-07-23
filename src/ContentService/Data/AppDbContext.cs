using ContentService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ContentService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Content> Contents => Set<Content>();
    }
}
