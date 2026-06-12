using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly AppDbContext _db;

        public UsersModel(AppDbContext db) => _db = db;

        public List<User> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        // Khoá / Mở tài khoản
        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null && user.Role != "Admin")
            {
                user.IsActive = !user.IsActive;
                await _db.SaveChangesAsync();
                TempData["Success"] = user.IsActive
                    ? $"Đã mở tài khoản \"{user.FullName}\""
                    : $"Đã khoá tài khoản \"{user.FullName}\"";
            }
            return RedirectToPage();
        }

        // Xoá tài khoản
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null && user.Role != "Admin")
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đã xoá tài khoản \"{user.FullName}\"";
            }
            return RedirectToPage();
        }
    }
}