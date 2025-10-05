using System.Net;
using FluentFTP;
using Microsoft.Extensions.Options;

namespace Drive_project.Stores
{
    public class FtpClientImpl : Drive_project.Services.IServices.IFtpClient
    {
        private readonly Config.FtpConfig _cfg;
        public FtpClientImpl(IOptions<Config.FtpConfig> cfg) => _cfg = cfg.Value;

        private async Task WithClient(Func<FluentFTP.FtpClient, Task> fn)
        {
            using var c = new FluentFTP.FtpClient
            {
                Host = _cfg.Host,
                Port = _cfg.Port,
                Credentials = new NetworkCredential(_cfg.User, _cfg.Pass),
            };

            if (_cfg.Secure)
            {
                c.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                c.Config.ValidateAnyCertificate = true;  
            }

             c.Connect();

            if (!string.IsNullOrWhiteSpace(_cfg.BaseDir))
            {
                 c.CreateDirectory(_cfg.BaseDir);
                 c.SetWorkingDirectory(_cfg.BaseDir);
            }

            await fn(c);
             c.Disconnect();
        }

        public async Task PutAsync(string id, byte[] data)
        {
            await WithClient(async c =>
            {
                var rel = id.TrimStart('/');
                var slash = rel.LastIndexOf('/');
                if (slash >= 0)
                {
                    var dir = rel[..slash];
                    c.CreateDirectory(dir);
                }

                using var ms = new MemoryStream(data, writable: false);
                var status =  c.UploadStream(
                    ms,
                    rel,
                    FtpRemoteExists.Overwrite,
                    true); 

                if (status is not (FtpStatus.Success or FtpStatus.Skipped))
                    throw new IOException($"FTP upload failed: {status}");
            });
        }


        public async Task<byte[]?> GetAsync(string id)
        {
            byte[]? result = null;

            await WithClient(async c =>
            {
                var rel = id.TrimStart('/');
                using var ms = new MemoryStream();
                var ok =  c.DownloadStream(ms, rel);
                if (!ok) throw new FileNotFoundException("FTP object not found");
                result = ms.ToArray();
            });

            return result;
        }
    }
}
