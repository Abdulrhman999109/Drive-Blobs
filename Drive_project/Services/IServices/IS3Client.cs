namespace Drive_project.Services.IServices
{
    public interface IS3Client
    {
        Task PutAsync(string key, byte[] data);
        Task<byte[]?> GetAsync(string key);
    }
}
