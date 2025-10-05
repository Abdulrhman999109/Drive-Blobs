using Drive_project.Models;
using Microsoft.EntityFrameworkCore;

namespace Drive_project.Data
{
    public class AppDbContext :DbContext
    {
        public DbSet<DataBlob> DataBlobs { get; set; }
        public DbSet<MetaBlob> MetaBlobs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
