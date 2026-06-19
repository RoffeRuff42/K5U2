using System.Text.Json.Serialization;

namespace LLM_Proxy_API.Services
{
    /// <summary>
    /// Typed HTTP client that generates text by calling a local Ollama instance.
    /// </summary>
    public class OllamaClient : ILlmClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        /// <summary>
        /// Creates a new instance of the client.
        /// </summary>
        /// <param name="httpClient">The pre-configured HTTP client pointing at the Ollama base URL.</param>
        /// <param name="config">Application configuration, used to read the model name.</param>
        public OllamaClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        /// <inheritdoc />
        public async Task<string> GenerateAsync(string prompt)
        {
            var model = _config["Llm:Model"] ?? "llama3.2";

            var ollamaRequest = new
            {
                model,
                prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("api/generate", ollamaRequest);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

            if (result == null)
            {
                throw new HttpRequestException("The external service returned an empty response.");
            }

            return result.Response;
        }

        private class OllamaResponse
        {
            [JsonPropertyName("response")]
            public string Response { get; set; } = string.Empty;
        }
    }
}
