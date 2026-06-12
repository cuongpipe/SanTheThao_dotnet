using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Pages.News
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db) => _db = db;

        public List<NewsPost> Posts { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _db.NewsPosts
                .Where(p => p.IsPublished)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            var query = _db.NewsPosts
                .Where(p => p.IsPublished)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Category))
                query = query.Where(p => p.Category == Category);

            Posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
