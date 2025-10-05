using Drive_project.Models;

namespace Drive_project.Repositories.IRepositories
{
    public interface IDataRepository
    {
        Task CreateAsync(DataBlob blob);
        Task<DataBlob?> GetAsync(string id);
    }
}
