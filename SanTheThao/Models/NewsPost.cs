namespace SanTheThao.Models
{
    public class NewsPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;       // URL thân thiện
        public string Summary { get; set; } = string.Empty;    // Tóm tắt
        public string Content { get; set; } = string.Empty;    // Nội dung đầy đủ
        public string? ThumbnailUrl { get; set; }              // Ảnh bìa
        public string Category { get; set; } = string.Empty;   // "Bóng đá", "Cầu lông"...
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;
    }
}