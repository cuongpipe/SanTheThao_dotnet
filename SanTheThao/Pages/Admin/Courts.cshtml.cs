using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CourtsModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CourtsModel(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public List<Court> Courts { get; set; } = new();
        public List<SportType> SportTypes { get; set; } = new();

        [BindProperty] public string CourtName { get; set; } = string.Empty;
        [BindProperty] public int SportTypeId { get; set; }
        [BindProperty] public decimal PricePerHour { get; set; }
        [BindProperty] public string Description { get; set; } = string.Empty;
        [BindProperty] public IFormFile? ImageFile { get; set; }

        public async Task OnGetAsync()
        {
            Courts = await _db.Courts
                .Include(c => c.SportType)
                .OrderBy(c => c.SportTypeId).ThenBy(c => c.Name)
                .ToListAsync();
            SportTypes = await _db.SportTypes.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            var court = new Court
            {
                Name = CourtName,
                SportTypeId = SportTypeId,
                PricePerHour = PricePerHour,
                Description = Description,
                IsActive = true
            };

            // Upload ảnh
            if (ImageFile != null && ImageFile.Length > 0)
                court.ImageUrl = await SaveImageAsync(ImageFile, $"court_{DateTime.Now.Ticks}");

            _db.Courts.Add(court);
            await _db.SaveChangesAsync();
            TempData["Message"] = $"Đã thêm sân \"{CourtName}\"";
            return RedirectToPage();
        }

        // Upload ảnh cho sân đã có
        public async Task<IActionResult> OnPostUploadImageAsync(int id)
        {
            var court = await _db.Courts.FindAsync(id);
            if (court == null) return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                court.ImageUrl = await SaveImageAsync(ImageFile, $"court_{id}");
                await _db.SaveChangesAsync();
                TempData["Message"] = $"Đã cập nhật ảnh sân \"{court.Name}\"";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var court = await _db.Courts.FindAsync(id);
            if (court != null)
            {
                court.IsActive = !court.IsActive;
                await _db.SaveChangesAsync();
                TempData["Message"] = court.IsActive ? $"Đã mở sân \"{court.Name}\"" : $"Đã ẩn sân \"{court.Name}\"";
            }
            return RedirectToPage();
        }

        private async Task<string> SaveImageAsync(IFormFile file, string name)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var folder = Path.Combine(_env.WebRootPath, "images", "courts");
            Directory.CreateDirectory(folder);
            var fileName = $"{name}{ext}";
            var path = Path.Combine(folder, fileName);
            using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);
            return $"/images/courts/{fileName}";
        }
    }
}