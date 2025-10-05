using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class JwtTests
{
    private static string IssueJwt(string id, TimeSpan expires)
    {
        var secret = "AbdulrhmanBalubaid_SecureJWTKey_2025!@#$";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim("id", id) },
            expires: DateTime.UtcNow.Add(expires),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public void Signs_token_that_verifies()
    {
        var token = IssueJwt("tester", TimeSpan.FromMinutes(5));

        var secret = "AbdulrhmanBalubaid_SecureJWTKey_2025!@#$";
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuerSigningKey = true
        }, out _);

        principal.Claims.First(c => c.Type == "id").Value.Should().Be("tester");
    }
}
