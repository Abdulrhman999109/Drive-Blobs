using Drive_project.Dto;

namespace Drive_project.Services.IServices
{
    public interface IBlobsService
    {
        Task<object> CreateBlobAsync(string id, string dataBase64, string backend);
        Task<BlobDto> GetAsync(string id);
    }
}
