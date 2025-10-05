namespace Drive_project.Config
{
    public class Storage
    {
        public string StorageDir { get; set; } = "./test/mest";

        public static void EnsureStorageDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
