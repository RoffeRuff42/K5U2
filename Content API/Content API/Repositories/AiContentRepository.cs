using Content_API.Data;
using Content_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
        /// Gets paginated items, optionally filtered by category and minimum creation date, and sorted by the given field.
        /// </summary>
        public async Task<(IEnumerable<AiContent> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? category, DateTime? startDate, string? sort)
        {
            var query = _context.AiContents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category == category);
            }

            if (startDate.HasValue)
            {
                query = query.Where(s => s.CreatedAt >= startDate.Value);
            }

            query = ApplySort(query, sort);

            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Applies sorting based on a field name, optionally prefixed with "-" for descending order (e.g. "-createdAt").
        /// Defaults to descending creation date when no sort is specified.
        /// </summary>
        private static IQueryable<AiContent> ApplySort(IQueryable<AiContent> query, string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
            {
                return query.OrderByDescending(s => s.CreatedAt);
            }

            var descending = sort.StartsWith('-');
            var field = descending ? sort[1..] : sort;

            return field.ToLowerInvariant() switch
            {
                "title" => descending ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
                "category" => descending ? query.OrderByDescending(s => s.Category) : query.OrderBy(s => s.Category),
                "createdat" => descending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt)
            };
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