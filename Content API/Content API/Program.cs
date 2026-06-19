using Content_API.Data;
using Content_API.Extensions;
using Content_API.Repositories;
using Content_API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using System.Net;

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

            // Configure Database Context (EF Core In-Memory)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("AiContentDb"));

            // Dependency Injection: Register Repositories
            builder.Services.AddScoped<IAiContentRepository, AiContentRepository>();

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
            builder.Services.ConfigureCors();

            // OpenAPI / Documentation setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            // --- Middleware Pipeline ---

            // 1. Exception Handling (must be first)
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exceptionHandlerFeature?.Error, "An unhandled exception occurred while processing the request.");

                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Title = "Internal Server Error",
                        Detail = "An unexpected error occurred. Please try again later."
                    };

                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });

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
            app.UseCors("CorsPolicy");
            app.UseAuthorization();

            // 4. Map Controller Endpoints
            app.MapControllers();

            app.Run();
        }
    }
}
