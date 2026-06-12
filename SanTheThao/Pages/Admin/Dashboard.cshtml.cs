using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _db;

        public DashboardModel(AppDbContext db) => _db = db;

        public int TotalCourts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<SanTheThao.Models.Booking> RecentBookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalCourts    = await _db.Courts.CountAsync(c => c.IsActive);
            TotalUsers     = await _db.Users.CountAsync(u => u.Role == "Customer");
            TotalBookings  = await _db.Bookings.CountAsync();
            PendingBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            TotalRevenue   = await _db.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .SumAsync(b => b.TotalPrice);

            RecentBookings = await _db.Bookings
                .Include(b => b.Court).ThenInclude(c => c.SportType)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}
