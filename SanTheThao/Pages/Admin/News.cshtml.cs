using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;
using System.Security.Claims;

namespace SanTheThao.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class NewsModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public NewsModel(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public List<NewsPost> Posts { get; set; } = new();

        [BindProperty] public string Title { get; set; } = string.Empty;
        [BindProperty] public string Slug { get; set; } = string.Empty;
        [BindProperty] public string Summary { get; set; } = string.Empty;
        [BindProperty] public string PostContent { get; set; } = string.Empty;
        [BindProperty] public string Category { get; set; } = string.Empty;
        [BindProperty] public IFormFile? ThumbnailFile { get; set; }

        public async Task OnGetAsync()
        {
            Posts = await _db.NewsPosts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            var authorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var slug = string.IsNullOrEmpty(Slug) ? ToSlug(Title) : Slug;

            string? thumbnailUrl = null;
            if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                thumbnailUrl = await SaveImageAsync(ThumbnailFile, $"news_{DateTime.Now.Ticks}");

            var post = new NewsPost
            {
                Title = Title,
                Slug = slug,
                Summary = Summary,
                Content = PostContent,
                Category = Category,
                ThumbnailUrl = thumbnailUrl,
                AuthorId = authorId,
                IsPublished = true,
                CreatedAt = DateTime.Now
            };

            _db.NewsPosts.Add(post);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã đăng bài \"{Title}\"";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var post = await _db.NewsPosts.FindAsync(id);
            if (post != null)
            {
                post.IsPublished = !post.IsPublished;
                await _db.SaveChangesAsync();
                TempData["Success"] = post.IsPublished ? "Đã đăng bài" : "Đã ẩn bài";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var post = await _db.NewsPosts.FindAsync(id);
            if (post != null)
            {
                _db.NewsPosts.Remove(post);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa bài viết";
            }
            return RedirectToPage();
        }

        private async Task<string> SaveImageAsync(IFormFile file, string name)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var folder = Path.Combine(_env.WebRootPath, "images", "news");
            Directory.CreateDirectory(folder);
            var fileName = $"{name}{ext}";
            var path = Path.Combine(folder, fileName);
            using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);
            return $"/images/news/{fileName}";
        }

        private static string ToSlug(string text) => text.ToLower()
            .Replace(" ", "-").Replace("đ", "d")
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẹ", "e").Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ọ", "o").Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ụ", "u").Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỵ", "y");
    }
}