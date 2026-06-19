using LLM_Proxy_API.DTOs;
using LLM_Proxy_API.Filters;
using LLM_Proxy_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLM_Proxy_API.Controllers
{
    /// <summary>
    /// Proxies text generation requests to a local Ollama instance on behalf of Service A.
    /// Protected by an internal API key validated by the <see cref="ApiKeyAttribute"/> filter.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiKey]
    public class LlmController : ControllerBase
    {
        private readonly ILlmClient _llmClient;

        /// <summary>
        /// Creates a new instance of the controller.
        /// </summary>
        /// <param name="llmClient">Typed client used to generate text via the local LLM.</param>
        public LlmController(ILlmClient llmClient)
        {
            _llmClient = llmClient;
        }

        /// <summary>
        /// Generates text from the given prompt using a local Ollama model.
        /// </summary>
        /// <param name="request">The generation request containing the prompt.</param>
        /// <returns>The generated text wrapped in a response object.</returns>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
        {
            var text = await _llmClient.GenerateAsync(request.Prompt);
            return Ok(new GenerateResponseDto(text));
        }
    }
}
