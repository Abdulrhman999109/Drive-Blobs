using Drive_project.Config;
using Drive_project.Services.IServices;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Drive_project.Stores
{
    public class S3Client : IS3Client
    {
        private readonly HttpClient _http;
        private readonly S3Config _cfg;

        public S3Client(HttpClient http, IOptions<S3Config> cfg)
        {
            _http = http;
            _cfg = cfg.Value;
        }

        private Uri BuildUrl(string key)
        {
            var scheme = _cfg.UseHttps ? "https" : "http";
            var encBucket = Uri.EscapeDataString(_cfg.Bucket ?? "");
            var encKey = string.Join("/", key.Split('/').Select(Uri.EscapeDataString));

            var baseHost = _cfg.Endpoint;
            var url = _cfg.PathStyle
                ? $"{scheme}://{baseHost}/{encBucket}/{encKey}"
                : $"{scheme}://{encBucket}.{baseHost}/{encKey}";
            return new Uri(url);
        }

        public async Task PutAsync(string key, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(_cfg.Bucket) ||
                string.IsNullOrWhiteSpace(_cfg.Access) ||
                string.IsNullOrWhiteSpace(_cfg.Secret))
                throw new InvalidOperationException("S3 is not configured");

            var url = BuildUrl(key);
            var payloadHash = S3SignatureHelper.Sha256Hex(data);
            var (amzDate, auth) = S3SignatureHelper.SignV4("PUT", url, _cfg.Region, _cfg.Access, _cfg.Secret, payloadHash);

            using var req = new HttpRequestMessage(HttpMethod.Put, url);
            req.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);
            req.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
            req.Headers.TryAddWithoutValidation("authorization", auth);
            req.Content = new ByteArrayContent(data);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var t = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new IOException($"S3 PUT failed: {(int)res.StatusCode} {res.ReasonPhrase} {t}");
            }
        }

        public async Task<byte[]?> GetAsync(string key)
        {
            var url = BuildUrl(key);
            var payloadHash = S3SignatureHelper.Sha256Hex("");
            var (amzDate, auth) = S3SignatureHelper.SignV4("GET", url, _cfg.Region, _cfg.Access, _cfg.Secret, payloadHash);

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);
            req.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
            req.Headers.TryAddWithoutValidation("authorization", auth);

            using var res = await _http.SendAsync(req);
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!res.IsSuccessStatusCode)
            {
                var t = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new IOException($"S3 GET failed: {(int)res.StatusCode} {res.ReasonPhrase} {t}");
            }
            return await res.Content.ReadAsByteArrayAsync();
        }
    }
}
