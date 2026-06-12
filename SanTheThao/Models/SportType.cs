namespace SanTheThao.Models
{
    public class SportType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;       // "Bóng đá", "Cầu lông"...
        public string Icon { get; set; } = string.Empty;       // "⚽", "🏸"...
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<Court> Courts { get; set; } = new List<Court>();
    }
}