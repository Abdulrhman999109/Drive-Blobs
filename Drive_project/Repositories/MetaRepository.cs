using Drive_project.Data;
using Drive_project.Models;
using Drive_project.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Drive_project.Repositories
{
    public class MetaRepository : IMetaRepository
    {
        private readonly AppDbContext _db;
        public MetaRepository(AppDbContext db) 
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(string id) =>
            await _db.MetaBlobs.AnyAsync(x => x.Id == id);

        public async Task CreateAsync(MetaBlob blob)
        {
            _db.MetaBlobs.Add(blob);
            await _db.SaveChangesAsync();
        }

        public async Task<MetaBlob?> GetAsync(string id)
        {
            return await _db.MetaBlobs.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
