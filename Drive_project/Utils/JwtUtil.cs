using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Drive_project.Utils
{
    public static class JwtUtil
    {
        public static string IssueJwt(IConfiguration config ,object user, int expiresInSeconds = 15 * 60 )
        {
            var secretKey = config["Jwt:SecretKey"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var handler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var claims = new Dictionary<string, object>
            {
                { "user", JsonSerializer.Serialize(user) }
            };
            var descriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                NotBefore = now,
                Expires = now.AddSeconds(expiresInSeconds),
                SigningCredentials = creds
            };
            var token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
