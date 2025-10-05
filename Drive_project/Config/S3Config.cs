namespace Drive_project.Config
{
    public class S3Config
    {
        public string Endpoint { get; set; } = "localhost:9000";
        public string Region { get; set; } = "us-east-1";
        public string Bucket { get; set; } = "bucket";
        public string Access { get; set; } = "minioadmin";
        public string Secret { get; set; } = "minioadmin";
        public bool PathStyle { get; set; } = true;
        public bool UseHttps { get; set; } = false;
    }
}
