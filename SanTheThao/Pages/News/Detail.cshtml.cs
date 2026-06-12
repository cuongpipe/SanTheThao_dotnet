using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;
using SanTheThao.Services;

namespace SanTheThao.Pages.News
{
    public class DetailModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly PostReviewService _postReviewService;

        public DetailModel(AppDbContext db, PostReviewService postReviewService)
        {
            _db = db;
            _postReviewService = postReviewService;
        }

        public NewsPost? Post { get; set; }
        public List<NewsPost> RelatedPosts { get; set; } = new();

        public List<PostReview> Reviews { get; set; } = new();

        [BindProperty]
        public PostReview NewReview { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Slug { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            Post = await _db.NewsPosts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == Slug && p.IsPublished);

            if (Post == null) return NotFound();

            // 🔥 FIX: dùng Post.Id
            Reviews = await _postReviewService.GetByPostIdAsync(Post.Id);

            RelatedPosts = await _db.NewsPosts
                .Where(p => p.Category == Post.Category && p.Id != Post.Id && p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Gán PostId trước khi lưu
            var post = await _db.NewsPosts
                .FirstOrDefaultAsync(p => p.Slug == Slug);

            if (post == null) return NotFound();

            NewReview.PostId = post.Id;

            await _postReviewService.AddAsync(NewReview);

            // 🔥 FIX: redirect theo Slug
            return RedirectToPage(new { slug = Slug });
        }
    }
}