using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.DTOs;
using SanTheThao.Models;

namespace SanTheThao.Services
{
    public interface IBookingService
    {
        Task<bool> IsCourtAvailableAsync(int courtId, DateOnly date, TimeOnly start, TimeOnly end);
        Task<List<Booking>> GetBookedSlotsAsync(int courtId, DateOnly date);
        Task<Booking> CreateBookingAsync(BookingDto dto);
        Task<List<Booking>> GetUserBookingsAsync(int userId);
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<bool> DeleteBookingAsync(int id); // THÊM KHAI BÁO HÀM XÓA
    }

    public class BookingService : IBookingService
    {
        private readonly AppDbContext _db;

        public BookingService(AppDbContext db) => _db = db;

        public async Task<bool> IsCourtAvailableAsync(int courtId, DateOnly date, TimeOnly start, TimeOnly end)
        {
            return !await _db.Bookings.AnyAsync(b =>
                b.CourtId == courtId &&
                b.BookingDate == date &&
                b.Status != BookingStatus.Cancelled &&
                b.StartTime < end &&
                b.EndTime > start);
        }

        public async Task<List<Booking>> GetBookedSlotsAsync(int courtId, DateOnly date)
            => await _db.Bookings
                .Where(b => b.CourtId == courtId &&
                            b.BookingDate == date &&
                            b.Status != BookingStatus.Cancelled)
                .ToListAsync();

        public async Task<Booking> CreateBookingAsync(BookingDto dto)
        {
            var court = await _db.Courts.FindAsync(dto.CourtId)
                ?? throw new Exception("Không tìm thấy sân");

            var hours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
            var totalPrice = court.PricePerHour * hours;

            var booking = new Booking
            {
                CourtId = dto.CourtId,
                UserId = dto.UserId,
                BookingDate = dto.BookingDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TotalPrice = totalPrice,
                Note = dto.Note,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<List<Booking>> GetUserBookingsAsync(int userId)
            => await _db.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Court)
                    .ThenInclude(c => c.SportType)
                .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.StartTime)
                .ToListAsync();

        public async Task<Booking?> GetBookingByIdAsync(int id)
            => await _db.Bookings
                .Include(b => b.Court)
                    .ThenInclude(c => c.SportType)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null || booking.Status == BookingStatus.Cancelled)
                return false;

            booking.Status = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();
            return true;
        }

        // TRIỂN KHAI HÀM XÓA ĐƠN HÀNG TẠM ĐỂ GIẢI PHÓNG SÂN
        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null) return false;

            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}