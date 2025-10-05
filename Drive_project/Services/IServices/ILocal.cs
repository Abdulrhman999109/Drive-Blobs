namespace Drive_project.Services.IServices
{
    public interface ILocal
    {
        Task PutAsync(string id, byte[] data);
        Task<byte[]?> GetAsync(string id);
    }
}
