using System.Security.Cryptography;
using System.Text;

namespace Drive_project.Config
{
    public class S3SignatureHelper
    {
        public static string Sha256Hex(byte[] data)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(data)).ToLowerInvariant();
        }
        public static string Sha256Hex(string s) => Sha256Hex(Encoding.UTF8.GetBytes(s));

        static byte[] HmacSha256(byte[] key, string data)
        {
            using var h = new HMACSHA256(key);
            return h.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        public static (string AmzDate, string Authorization) SignV4(
            string method, Uri url, string region, string access, string secret,
            string payloadHash, string? amzDateOverride = null)
        {
            var now = DateTimeOffset.UtcNow;
            var amzDate = amzDateOverride ?? now.ToString("yyyyMMdd'T'HHmmss'Z'");
            var shortDate = amzDate[..8];

            var query = System.Web.HttpUtility.ParseQueryString(url.Query);
            var keys = query.AllKeys!.Where(k => k != null).Select(k => k!).OrderBy(k => k, StringComparer.Ordinal);
            var canonicalQuery = string.Join("&", keys.Select(k =>
                $"{Uri.EscapeDataString(k)}={Uri.EscapeDataString(query[k] ?? string.Empty)}"));

            var host = url.IsDefaultPort ? url.Host : $"{url.Host}:{url.Port}";
            var signedHeaders = "host;x-amz-content-sha256;x-amz-date";
            var canonicalHeaders =
                $"host:{host}\n" +
                $"x-amz-content-sha256:{payloadHash}\n" +
                $"x-amz-date:{amzDate}\n";

            var canonicalRequest =
                $"{method}\n{url.AbsolutePath}\n{canonicalQuery}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
            var scope = $"{shortDate}/{region}/s3/aws4_request";
            var stringToSign =
                $"AWS4-HMAC-SHA256\n{amzDate}\n{scope}\n{Sha256Hex(canonicalRequest)}";

            var kDate = HmacSha256(Encoding.UTF8.GetBytes("AWS4" + secret), shortDate);
            var kRegion = HmacSha256(kDate, region);
            var kService = HmacSha256(kRegion, "s3");
            var kSigning = HmacSha256(kService, "aws4_request");
            using var hmac = new HMACSHA256(kSigning);
            var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign))).ToLowerInvariant();

            var authorization =
                $"AWS4-HMAC-SHA256 Credential={access}/{scope}, SignedHeaders={signedHeaders}, Signature={signature}";

            return (amzDate, authorization);
        }

    }
}
