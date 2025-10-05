using System;
using System.Linq; // مهم لـ SingleOrDefault
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Drive_project.Data;
using Xunit;

namespace Drive_project.Tests.Integration
{
    public class TestWebAppFactory : WebApplicationFactory<global::Drive_project.Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase($"BlobsTestDb-{Guid.NewGuid():N}"));
            });
        }
    }

    public class BlobDbTests : IClassFixture<TestWebAppFactory>
    {
        private readonly HttpClient _client;

        public BlobDbTests(TestWebAppFactory factory )
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("JWT_SECRET", "AbdulrhmanBalubaid_SecureJWTKey_2025!@#$");

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
            var config = factory.Services.GetRequiredService<IConfiguration>();
            var token = IssueJwt("tester" , config);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private static string IssueJwt(string id ,IConfiguration config)
        {
            var secret = config["Jwt:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[] { new Claim("id", id) },
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task CreateBlob_ShouldReturnCreated()
        {
            var id = Guid.NewGuid().ToString("N");
            var dataB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World"));
            var backend = "db";
            var createReq = new { id, data = dataB64, backend };

            var response = await _client.PostAsJsonAsync("/v1/blobs", createReq);
            var raw = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Created, $"response: {raw}");
            var body = JsonDocument.Parse(raw).RootElement;
            body.GetProperty("success").GetBoolean().Should().BeTrue();
        }

    }
}
