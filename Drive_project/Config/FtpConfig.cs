namespace Drive_project.Config
{
    public class FtpConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 2121;
        public string User { get; set; } = "dev";
        public string Pass { get; set; } = "devpass";
        public bool Secure { get; set; } = false;
        public string BaseDir { get; set; } = "/";

    }
}
