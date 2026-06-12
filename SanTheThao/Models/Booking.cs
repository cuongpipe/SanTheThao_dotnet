namespace SanTheThao.Models
{
    public enum BookingStatus
    {
        Pending,      // Chờ xác nhận
        Confirmed,    // Đã xác nhận
        Cancelled     // Đã hủy
    }

    public class Booking
    {
        public int Id { get; set; }

        public int CourtId { get; set; }
        public Court Court { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateOnly BookingDate { get; set; }       // Ngày đặt
        public TimeOnly StartTime { get; set; }         // Giờ bắt đầu
        public TimeOnly EndTime { get; set; }           // Giờ kết thúc

        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}