using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace Drive_project.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services , IConfiguration config)
        {
            var jwtSecret = config["Jwt:SecretKey"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.Zero
                    };

                    opt.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx =>
                        {
                            try
                            {
                                var userJson = ctx.Principal?.FindFirst("user")?.Value;
                                if (!string.IsNullOrWhiteSpace(userJson))
                                {
                                    using var doc = JsonDocument.Parse(userJson);
                                    if (doc.RootElement.TryGetProperty("id", out var idProp))
                                    {
                                        var id = idProp.GetString();
                                        if (!string.IsNullOrWhiteSpace(id))
                                        {
                                            var identity = (System.Security.Claims.ClaimsIdentity)ctx.Principal!.Identity!;
                                            identity.AddClaim(new System.Security.Claims.Claim("id", id!));
                                        }
                                    }
                                }
                            }
                            catch { }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
