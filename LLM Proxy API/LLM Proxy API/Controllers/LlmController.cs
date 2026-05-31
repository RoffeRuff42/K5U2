using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace LLM_Proxy_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LlmController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public LlmController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            // Free API client for fetching jokes
            _httpClient = httpClientFactory.CreateClient("FreeClient");
            _config = config;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> Generate([FromQuery] string prompt)
        {
            // Validate the call from Service A
            var expectedKey = _config["InternalApiKey"];
            if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedKey) || extractedKey != expectedKey)
            {
                return Unauthorized("Invalid internal API key.");
            }

            // Make a call to the external Joke API
            var response = await _httpClient.GetAsync("random_joke");
            response.EnsureSuccessStatusCode();

            // Read the JSON response and map it to our JokeResponse class
            var result = await response.Content.ReadFromJsonAsync<JokeResponse>();

            if (result != null)
            {
                // Combine the setup and punchline into a single joke string
                var finalJoke = $"{result.Setup} {result.Punchline}";
                return Ok(new { Response = finalJoke });
            }

            throw new HttpRequestException("The external service returned an empty response.");
        }
    }

    // Class to represent the structure of the joke response from the external API
    public class JokeResponse
    {
        [JsonPropertyName("setup")]
        public string Setup { get; set; } = string.Empty;

        [JsonPropertyName("punchline")]
        public string Punchline { get; set; } = string.Empty;
    }
}