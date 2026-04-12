using Content_API.Data;
using Content_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Content_API.Repositories
{
    public class AiContentRepository : IAiContentRepository
    {
        private readonly ApplicationDbContext _context;

        public AiContentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets paginated items, optionally filtered by category.
        /// </summary>
        public async Task<(IEnumerable<AiContent> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? category)
        {
            var query = _context.AiContents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category == category);
            }

            var totalCount = await query.CountAsync();

            // Apply sorting and pagination
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<AiContent?> GetByIdAsync(int id) =>
            await _context.AiContents.FirstOrDefaultAsync(s => s.Id == id);

        public async Task AddAsync(AiContent aiContent)
        {
            await _context.AiContents.AddAsync(aiContent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AiContent aiContent)
        {
            _context.AiContents.Update(aiContent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var aiContent = await GetByIdAsync(id);
            if (aiContent != null)
            {
                _context.AiContents.Remove(aiContent);
                await _context.SaveChangesAsync();
            }
        }
    }
}