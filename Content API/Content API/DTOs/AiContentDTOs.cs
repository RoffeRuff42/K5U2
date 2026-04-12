using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Content_API.DTOs
{
    public record AiContentReadDto(
        int Id,
        string Title,
        string OriginalPrompt,
        string GeneratedText,
        string Category,
        DateTime CreatedAt
    );

    public record AiContentCreateDto(
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2)]
        string Title,

        [Required(ErrorMessage = "A prompt is needed to generate content")]
        [StringLength(2000)]
        string OriginalPrompt,

        [Required]
        string Category
    );

    public record AiContentUpdateDto(
        [Required]
        [StringLength(100)]
        string Title,

        [Required]
        [StringLength(2000)]
        string OriginalPrompt,

        [Required]
        string Category
    );

    public record PagedResponse<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );
}
