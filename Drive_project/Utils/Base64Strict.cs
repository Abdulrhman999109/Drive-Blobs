using System.Text.RegularExpressions;

namespace Drive_project.Utils
{
    public class Base64Strict
    {
        private static readonly Regex ValidChars = new("^[A-Za-z0-9+/]*={0,2}$", RegexOptions.Compiled);

        private static string Normalize(string input)
        {
            var s = input.Contains(',') ? input[(input.IndexOf(',') + 1)..] : input;
            return s.Replace("-", "+").Replace("_", "/").Replace(" ", "");
        }

        public static byte[] DecodeOrThrow(string? b64)
        {
            if (string.IsNullOrWhiteSpace(b64))
                throw new ArgumentException("Invalid Base64 payload: empty");

            var s = Normalize(b64);

            if (!ValidChars.IsMatch(s))
                throw new ArgumentException("Invalid Base64 payload: illegal characters");

            if (s.Length % 4 != 0)
                throw new ArgumentException("Invalid Base64 payload: bad padding length");

            byte[] buf;
            try
            {
                buf = Convert.FromBase64String(s);
            }
            catch
            {
                throw new ArgumentException("Invalid Base64 payload: decode error");
            }

            if (buf.Length == 0)
                throw new ArgumentException("Invalid Base64 payload: decoded empty");

            if (Convert.ToBase64String(buf) != s)
                throw new ArgumentException("Invalid Base64 payload: roundtrip mismatch");

            return buf;
        }
    }
}
