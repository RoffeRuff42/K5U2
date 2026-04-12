using System;
using System.ComponentModel.DataAnnotations;

namespace Content_API.Models
{
    public class AiContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty; // Name of the content

        [Required]
        [StringLength(2000)]
        public string OriginalPrompt { get; set; } = string.Empty; // Users prompt that was sent to Service B

        [Required]
        public string GeneratedText { get; set; } = string.Empty; // AI-generated content from Service B

        [Required]
        public string Category { get; set; } = "General"; // Category of the content, e.g., "Article", "Blog Post", "Product Description"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
