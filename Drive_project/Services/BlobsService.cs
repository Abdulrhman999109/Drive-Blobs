using Drive_project.Dto;
using Drive_project.Models;
using Drive_project.Repositories.IRepositories;
using Drive_project.Services.IServices;
using Drive_project.Utils;

namespace Drive_project.Services
{
    public class BlobsService : IBlobsService
    {
        private readonly IMetaRepository _metaRepo;
        private readonly IDataRepository _dataRepo;
        private readonly ILocal _local;
        private readonly IS3Client _s3;
        private readonly IFtpClient _ftp;

        public BlobsService(
            IMetaRepository metaRepo,
            IDataRepository dataRepo,
            ILocal local,
            IS3Client s3,
            IFtpClient ftp)
        {
            _metaRepo = metaRepo;
            _dataRepo = dataRepo;
            _local = local;
            _s3 = s3;
            _ftp = ftp;
        }

        public async Task<object> CreateBlobAsync(string id, string dataBase64, string backend)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(dataBase64) ||
                string.IsNullOrWhiteSpace(backend))
                throw HttpEx(400, "All fields are required");

            var b = backend.Trim().ToLowerInvariant();
            if (b is not ("db" or "local" or "s3" or "ftp"))
                throw HttpEx(400, "unsupported backend");

            var buffer = Base64Strict.DecodeOrThrow(dataBase64);
            var size = buffer.Length;

            if (await _metaRepo.ExistsAsync(id))
                throw HttpEx(400, "id already exists");

            switch (b)
            {
                case "db":
                    await _dataRepo.CreateAsync(new DataBlob { Id = id, Data = buffer });
                    break;

                case "local":
                    await _local.PutAsync(id, buffer);
                    break;

                case "s3":
                    await _s3.PutAsync(id, buffer);
                    break;

                case "ftp":
                    await _ftp.PutAsync(id, buffer);
                    break;
            }

            await _metaRepo.CreateAsync(new MetaBlob
            {
                Id = id,
                Backend = b,
                Size = size,
                CreatedAt = DateTime.UtcNow
            });

            return new { id, size, backend = b };
        }

        public async Task<BlobDto> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw HttpEx(400, "id is required");

            var meta = await _metaRepo.GetAsync(id);
            if (meta is null)
                throw HttpEx(404, "Blob not found");

            byte[]? dataBytes = meta.Backend switch
            {
                "db" => (await _dataRepo.GetAsync(id))?.Data
                        ?? throw HttpEx(404, "Blob data not found"),

                "local" => await _local.GetAsync(id)
                           ?? throw HttpEx(502, "failed to read local blob"),

                "s3" => await _s3.GetAsync(id)
                       ?? throw HttpEx(502, "failed to read from S3"),

                "ftp" => await _ftp.GetAsync(id)
                        ?? throw HttpEx(502, "failed to read from FTP"),

                _ => throw HttpEx(400, "unsupported backend")
            };

            return new BlobDto
            {
                Id = meta.Id,
                Data = Convert.ToBase64String(dataBytes),
                Size = meta.Size,
                CreatedAt = meta.CreatedAt,
                Backend = meta.Backend
            };
        }

        private static Exception HttpEx(int code, string message)
        {
            var ex = new Exception(message);
            ex.Data["StatusCode"] = code; 
            return ex;
        }
    }
}
