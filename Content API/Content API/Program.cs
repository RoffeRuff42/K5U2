using Content_API.Data;
using Content_API.Extensions;
using Content_API.Repositories;
using Content_API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Content_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load API Settings for Service B communication from Configuration/User Secrets
            var apiSettings = builder.Configuration.GetSection("ApiSettings");
            var baseAddress = apiSettings.GetValue<string>("BaseAddress") ?? "https://localhost:7005/";
            var apiKey = apiSettings.GetValue<string>("ApiKey") ?? string.Empty;

            // Configure Database Context (SQL Server)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Dependency Injection: Register Repositories and Services
            builder.Services.AddScoped<IAiContentRepository, AiContentRepository>();
            builder.Services.AddScoped<IAiContentService, AiContentService>();

            // Configure Typed HttpClient for Service B with security headers
            builder.Services.AddHttpClient<IAiContentService, AiContentService>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
                }
            });

            // Infrastructure services
            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();

            // OpenAPI / Documentation setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // --- Middleware Pipeline ---

            // 1. Exception Handling (must be first)
            app.UseExceptionHandler("/error");

            // 2. Documentation UI (Development only)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "openapi/{documentName}.json";
                });

                app.MapScalarApiReference();
            }

            // 3. Security and Routing
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            // 4. Map Controller Endpoints
            app.MapControllers();

            app.Run();
        }
    }
}
