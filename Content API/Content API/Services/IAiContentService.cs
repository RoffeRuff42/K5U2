using Content_API.DTOs;
using System;
using System.Threading.Tasks;

namespace Content_API.Services
{
    public interface IAiContentService
    {
        Task<PagedResponse<AiContentReadDto>> GetAiContentsAsync(int page, int pageSize, string? category, DateTime? startDate, string? sort);

        Task<AiContentReadDto?> GetAiContentByIdAsync(int id);

        Task<AiContentReadDto> CreateAiContentAsync(AiContentCreateDto createDto);

        Task<bool> UpdateAiContentAsync(int id, AiContentUpdateDto updateDto);

        Task<bool> DeleteAiContentAsync(int id);
    }
}

