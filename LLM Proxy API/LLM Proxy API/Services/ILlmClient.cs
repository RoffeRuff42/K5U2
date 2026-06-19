namespace LLM_Proxy_API.Services
{
    /// <summary>
    /// Abstraction for generating text from a local large language model.
    /// </summary>
    public interface ILlmClient
    {
        /// <summary>
        /// Generates text for the given prompt.
        /// </summary>
        /// <param name="prompt">The prompt to send to the model.</param>
        /// <returns>The generated text.</returns>
        Task<string> GenerateAsync(string prompt);
    }
}
