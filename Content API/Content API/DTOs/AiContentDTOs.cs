using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Content_API.DTOs
{
    /// <summary>
    /// Represents an AI content item as returned to clients.
    /// </summary>
    /// <param name="Id">The unique identifier of the content item.</param>
    /// <param name="Title">The title of the content item.</param>
    /// <param name="OriginalPrompt">The original prompt used to generate the content.</param>
    /// <param name="GeneratedText">The AI-generated text.</param>
    /// <param name="Category">The category of the content item.</param>
    /// <param name="CreatedAt">The date and time the content item was created.</param>
    public record AiContentReadDto(
        int Id,
        string Title,
        string OriginalPrompt,
        string GeneratedText,
        string Category,
        DateTime CreatedAt
    );

    /// <summary>
    /// Represents the data required to create new AI content.
    /// </summary>
    /// <param name="Title">The title of the content item.</param>
    /// <param name="OriginalPrompt">The prompt used to generate the content.</param>
    /// <param name="Category">The category of the content item.</param>
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

    /// <summary>
    /// Represents the data required to update existing AI content.
    /// </summary>
    /// <param name="Title">The title of the content item.</param>
    /// <param name="OriginalPrompt">The prompt used to generate the content.</param>
    /// <param name="Category">The category of the content item.</param>
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

    /// <summary>
    /// Represents a paginated response.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the page.</typeparam>
    /// <param name="Items">The items on the current page.</param>
    /// <param name="Page">The current page number.</param>
    /// <param name="PageSize">The number of items per page.</param>
    /// <param name="TotalCount">The total number of items across all pages.</param>
    /// <param name="TotalPages">The total number of pages.</param>
    public record PagedResponse<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );
}
