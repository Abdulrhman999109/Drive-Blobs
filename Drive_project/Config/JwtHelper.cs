using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Drive_project.Config
{
    public class JwtHelper
    {
        public static string IssueDevToken(IConfiguration config)
        {
            var secret = config["Jwt:SecretKey"];
            var expires = "15m"; 
            var seconds = ParseExpires(expires); 

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTimeOffset.UtcNow;
            var payload = new JwtPayload
            {
                { "user", new Dictionary<string, object> { { "id", "client" } } },
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", now.AddSeconds(seconds).ToUnixTimeSeconds() }
            };

            var token = new JwtSecurityToken(new JwtHeader(creds), payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static int ParseExpires(string v)
        {
            if (int.TryParse(v, out var s)) return s;
            if (v.EndsWith("m") && int.TryParse(v[..^1], out var m)) return m * 60;
            if (v.EndsWith("h") && int.TryParse(v[..^1], out var h)) return h * 3600;
            return 15 * 60;
        }   
    }
}
