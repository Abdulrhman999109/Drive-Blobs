namespace Drive_project.Services.IServices
{
    public interface IFtpClient
    {
        Task PutAsync(string id, byte[] data);
        Task<byte[]?> GetAsync(string id);
    }
}
