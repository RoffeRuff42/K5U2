
using LLM_Proxy_API.Controllers;

namespace LLM_Proxy_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient("FreeClient", (serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = config["FreeApi:BaseUrl"];

                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            var app = builder.Build();

            // Enable the custom exception handling middleware to catch all unhandled errors
            app.UseMiddleware<LLM_Proxy_API.Middlewares.ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
