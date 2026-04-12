using Content_API.DTOs;
using Content_API.Models;
using Content_API.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Content_API.Services
{
    /// <summary>
    /// Service responsible for handling business logic for AI content, 
    /// including caching and communication with the LLM Proxy (Service B).
    /// </summary>
    public class AiContentService : IAiContentService
    {
        private readonly IAiContentRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private const string CacheKeyPrefix = "AiContents_list";

        public AiContentService(IAiContentRepository repository, IMemoryCache cache, HttpClient httpClient)
        {
            _repository = repository;
            _cache = cache;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves paginated content with memory caching for improved performance.
        /// </summary>
        public async Task<PagedResponse<AiContentReadDto>> GetAiContentsAsync(int page, int pageSize, string? category)
        {
            string cacheKey = $"{CacheKeyPrefix}_p{page}_s{pageSize}_c{category ?? "all"}";

            // Attempt to retrieve data from cache
            if (!_cache.TryGetValue(cacheKey, out PagedResponse<AiContentReadDto>? cachedResponse))
            {
                var (items, totalCount) = await _repository.GetAllAsync(page, pageSize, category);

                var dtos = items.Select(s => new AiContentReadDto(
                    s.Id, s.Title, s.OriginalPrompt, s.GeneratedText, s.Category, s.CreatedAt
                ));

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                cachedResponse = new PagedResponse<AiContentReadDto>(dtos, page, pageSize, totalCount, totalPages);

                // Set cache expiration: 5 minutes absolute, 2 minutes sliding
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, cachedResponse, cacheOptions);
            }

            return cachedResponse!;
        }

        /// <summary>
        /// Gets a single content item by ID.
        /// </summary>
        public async Task<AiContentReadDto?> GetAiContentByIdAsync(int id)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null) return null;

            return new AiContentReadDto(s.Id, s.Title, s.OriginalPrompt, s.GeneratedText, s.Category, s.CreatedAt);
        }

        /// <summary>
        /// Creates new content by calling Service B (LLM Proxy) and saving the result to the database.
        /// Includes error handling and fallback logic for the external AI call.
        /// </summary>
        public async Task<AiContentReadDto> CreateAiContentAsync(AiContentCreateDto createDto)
        {
            string aiResponseText;

            try
            {
                // Call Service B to generate content based on the prompt
                var response = await _httpClient.GetAsync($"api/Llm/generate?prompt={Uri.EscapeDataString(createDto.OriginalPrompt)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LlmResponseDTO>();
                    aiResponseText = result?.Response ?? "AI was unable to respond.";
                }
                else
                {
                    // Log specific error from Service B if the call fails
                    var errorBody = await response.Content.ReadAsStringAsync();
                    aiResponseText = $"Service B Error ({response.StatusCode}): {errorBody}";
                }
            }
            catch (Exception ex)
            {
                // Fallback logic in case of network issues or timeouts
                aiResponseText = $"Critical Connection Error: {ex.Message}";
            }

            var newAiContent = new AiContent
            {
                Title = createDto.Title,
                OriginalPrompt = createDto.OriginalPrompt,
                Category = createDto.Category,
                GeneratedText = aiResponseText,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newAiContent);

            // Clear cache to ensure the list reflects the new data
            ClearAiContentCache();

            return new AiContentReadDto(
                newAiContent.Id, newAiContent.Title, newAiContent.OriginalPrompt,
                newAiContent.GeneratedText, newAiContent.Category, newAiContent.CreatedAt
            );
        }

        /// <summary>
        /// Updates content and invalidates the cache.
        /// </summary>
        public async Task<bool> UpdateAiContentAsync(int id, AiContentUpdateDto updateDto)
        {
            var content = await _repository.GetByIdAsync(id);
            if (content == null) return false;

            content.Title = updateDto.Title;
            content.OriginalPrompt = updateDto.OriginalPrompt;
            content.Category = updateDto.Category;
            content.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(content);
            ClearAiContentCache();

            return true;
        }

        /// <summary>
        /// Deletes content and invalidates the cache.
        /// </summary>
        public async Task<bool> DeleteAiContentAsync(int id)
        {
            var content = await _repository.GetByIdAsync(id);
            if (content == null) return false;

            await _repository.DeleteAsync(content.Id);
            ClearAiContentCache();

            return true;
        }

        /// <summary>
        /// Helper method to clear specific cache entries.
        /// </summary>
        private void ClearAiContentCache()
        {
            // Simple removal of the first page cache to trigger a refresh
            _cache.Remove($"{CacheKeyPrefix}_p1_s10_call");
        }

        private class LlmResponseDTO { public string Response { get; set; } = string.Empty; }
    }
}
