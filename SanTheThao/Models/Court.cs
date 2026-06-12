namespace SanTheThao.Models
{
    public class Court
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;          // "Sân A1", "Sân B2"
        public string Description { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public int SportTypeId { get; set; }
        public SportType SportType { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}