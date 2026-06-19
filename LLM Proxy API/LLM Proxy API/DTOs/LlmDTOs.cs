namespace LLM_Proxy_API.DTOs
{
    /// <summary>
    /// Represents a request to generate text from a prompt.
    /// </summary>
    /// <param name="Prompt">The prompt to send to the language model.</param>
    public record GenerateRequest(string Prompt);

    /// <summary>
    /// Represents the response returned to the caller after text generation.
    /// </summary>
    /// <param name="Response">The generated text.</param>
    public record GenerateResponseDto(string Response);
}
