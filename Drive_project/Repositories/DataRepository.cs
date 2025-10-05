using Drive_project.Data;
using Drive_project.Models;
using Drive_project.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Drive_project.Repositories
{
    public class DataRepository : IDataRepository
    {
        private readonly AppDbContext _db;
        public DataRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(DataBlob blob)
        {
            _db.DataBlobs.Add(blob);
            await _db.SaveChangesAsync();
        }

        public async Task<DataBlob?> GetAsync(string id)
        {
            return await _db.DataBlobs.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
