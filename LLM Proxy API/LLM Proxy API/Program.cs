
using LLM_Proxy_API.Services;
using Scalar.AspNetCore;

namespace LLM_Proxy_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // OpenAPI / Documentation setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Typed HttpClient for calling the local Ollama instance (avoids socket exhaustion)
            builder.Services.AddHttpClient<ILlmClient, OllamaClient>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = config["Llm:BaseUrl"] ?? "http://localhost:11434/";

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            var app = builder.Build();

            // Enable the custom exception handling middleware to catch all unhandled errors
            app.UseMiddleware<LLM_Proxy_API.Middlewares.ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "openapi/{documentName}.json";
                });

                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
