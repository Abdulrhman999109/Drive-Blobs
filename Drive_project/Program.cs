using Drive_project.Data;
using Drive_project.Repositories.IRepositories;
using Drive_project.Repositories;
using Drive_project.Services.IServices;
using Drive_project.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Drive_project.Middleware;
using Drive_project.Config;
using Drive_project.Stores;
using Drive_project.Extensions; 

namespace Drive_project
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("BlobsDb")));

            // Repositories
            builder.Services.AddScoped<IMetaRepository, MetaRepository>();
            builder.Services.AddScoped<IDataRepository, DataRepository>();

            // Services
            builder.Services.AddScoped<IBlobsService, BlobsService>();

            // Configs 
            builder.Services.Configure<Storage>(builder.Configuration.GetSection("Storage"));
            builder.Services.Configure<FtpConfig>(builder.Configuration.GetSection("Ftp"));
            builder.Services.Configure<S3Config>(builder.Configuration.GetSection("S3"));

            // Stores
            builder.Services.AddScoped<ILocal, Local>();
            builder.Services.AddScoped<IFtpClient, FtpClientImpl>();
            builder.Services.AddHttpClient<IS3Client, S3Client>();

            builder.Services.AddJwtAuth(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Drive Project API",
                    Version = "v1"
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter '{token}'"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };
                c.AddSecurityRequirement(securityRequirement);
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseJsonErrorHandler();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseJsonNotFound();
            app.Run();
    }
    }
}
