using Content_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Content_API.Repositories
{
    public interface IAiContentRepository
    {
        Task<(IEnumerable<AiContent> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? category);
        Task<AiContent?> GetByIdAsync(int id);
        Task AddAsync(AiContent aiContent);
        Task UpdateAsync(AiContent aiContent);
        Task DeleteAsync(int id);
    }
}
