using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BookingsModel : PageModel
    {
        private readonly AppDbContext _db;

        public BookingsModel(AppDbContext db) => _db = db;

        public List<SanTheThao.Models.Booking> Bookings { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _db.Bookings
                .Include(b => b.Court).ThenInclude(c => c.SportType)
                .Include(b => b.User)
                .AsQueryable();

            if (StatusFilter == "Pending")
                query = query.Where(b => b.Status == BookingStatus.Pending);
            else if (StatusFilter == "Confirmed")
                query = query.Where(b => b.Status == BookingStatus.Confirmed);
            else if (StatusFilter == "Cancelled")
                query = query.Where(b => b.Status == BookingStatus.Cancelled);

            Bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking != null && booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.Confirmed;
                await _db.SaveChangesAsync();
                TempData["Message"] = "Đã xác nhận đơn #" + id.ToString("D6");
            }
            return RedirectToPage(new { StatusFilter });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking != null && booking.Status != BookingStatus.Cancelled)
            {
                booking.Status = BookingStatus.Cancelled;
                await _db.SaveChangesAsync();
                TempData["Message"] = "Đã hủy đơn #" + id.ToString("D6");
            }
            return RedirectToPage(new { StatusFilter });
        }
    }
}
