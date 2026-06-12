using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;


        public IndexModel(AppDbContext db) => _db = db;

        public List<SportType> SportTypes { get; set; } = new();
        public List<(Court Court, int BookingCount)> TopCourts { get; set; } = new();
        public List<NewsPost> LatestNews { get; set; } = new();

        public async Task OnGetAsync()
        {
            SportTypes = await _db.SportTypes
                .Where(s => s.IsActive)
                .Include(s => s.Courts)
                .ToListAsync();

            // Top 3 sân được đặt nhiều nhất
            var topCourts = await _db.Courts
                .Where(c => c.IsActive)
                .Include(c => c.SportType)
                .Include(c => c.Bookings)
                .Select(c => new
                {
                    Court = c,
                    BookingCount = c.Bookings.Count(b => b.Status != BookingStatus.Cancelled)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(3)
                .ToListAsync();

            TopCourts = topCourts.Select(x => (x.Court, x.BookingCount)).ToList();

            // 3 tin tức mới nhất
            LatestNews = await _db.NewsPosts
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();
        }
    }
}