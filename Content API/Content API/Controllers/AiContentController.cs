using Content_API.DTOs;
using Content_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Content_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiContentController : ControllerBase
    {
        private readonly IAiContentService _service;

        public AiContentController(IAiContentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a paginated list of generated content.
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="category">Filter by category</param>
        [HttpGet] // GET: api/AiContent
        public async Task<ActionResult<PagedResponse<AiContentReadDto>>> GetAiContents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null)
        {
            var response = await _service.GetAiContentsAsync(page, pageSize, category);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves specific content by ID.
        /// </summary>
        /// <param name="id">Content ID</param>
        /// <returns>The specific content item</returns>
        [HttpGet("{id}")] // GET: api/AiContent/5
        public async Task<ActionResult<AiContentReadDto>> GetAiContent(int id)
        {
            var content = await _service.GetAiContentByIdAsync(id);
            if (content == null) return NotFound();

            return Ok(content);
        }

        /// <summary>
        /// Creates new AI content by calling Service B.
        /// </summary>
        /// <param name="createDto">Data for the content to be generated</param>
        [HttpPost] // POST: api/AiContent
        public async Task<ActionResult<AiContentReadDto>> CreateAiContent(AiContentCreateDto createDto)
        {
            var result = await _service.CreateAiContentAsync(createDto);
            return CreatedAtAction(nameof(GetAiContent), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates existing content.
        /// </summary>
        /// <param name="id">ID of the content to update</param>
        /// <param name="updateDto">New data for the content</param>
        [HttpPut("{id}")] // PUT: api/AiContent/5
        public async Task<IActionResult> UpdateAiContent(int id, AiContentUpdateDto updateDto)
        {
            var success = await _service.UpdateAiContentAsync(id, updateDto);
            if (!success) return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes content from the database.
        /// </summary>
        /// <param name="id">ID of the content to delete</param>
        [HttpDelete("{id}")] // DELETE: api/AiContent/5
        public async Task<IActionResult> DeleteAiContent(int id)
        {
            var success = await _service.DeleteAiContentAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}