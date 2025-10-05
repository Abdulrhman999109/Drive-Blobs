using Drive_project.Config;             
using Drive_project.Services.IServices; 
using Microsoft.Extensions.Options;

namespace Drive_project.Stores
{
    public class Local : ILocal
    {
        private readonly string _root;

        public Local(IOptions<Storage> cfg)
        {
            _root = Path.GetFullPath(cfg.Value.StorageDir ?? "./test/mest");
            Storage.EnsureStorageDir(_root);
        }

        private string FullPathSafe(string id)
        {
            var baseDir = _root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var rel = id.TrimStart('/', '\\');
            var full = Path.GetFullPath(Path.Combine(_root, rel));
            if (!full.StartsWith(baseDir, StringComparison.Ordinal))
                throw new ArgumentException("Invalid id");
            return full;
        }

        public async Task PutAsync(string id, byte[] data)
        {
            var path = FullPathSafe(id);
            var dir = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dir);

            if (File.Exists(path)) throw new IOException("file already exists");

            await File.WriteAllBytesAsync(path, data);
            if (new FileInfo(path).Length != data.LongLength)
                throw new IOException("written file size mismatch");
        }

        public async Task<byte[]?> GetAsync(string id)
        {
            var path = FullPathSafe(id);
            if (!File.Exists(path)) return null;
            return await File.ReadAllBytesAsync(path);
        }
    }
}