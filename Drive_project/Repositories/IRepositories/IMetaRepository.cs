using Drive_project.Models;

namespace Drive_project.Repositories.IRepositories
{
    public interface IMetaRepository
    {
        Task<bool> ExistsAsync(string id);
        Task CreateAsync(MetaBlob blob);
        Task<MetaBlob?> GetAsync(string id);
    }
}
