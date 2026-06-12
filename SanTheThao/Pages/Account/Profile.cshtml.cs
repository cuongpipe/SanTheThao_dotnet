using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace SanTheThao.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _db;

        public ProfileModel(AppDbContext db) => _db = db;

        [BindProperty]
        public ProfileInput Input { get; set; } = new();

        [BindProperty]
        public PasswordInput PasswordChange { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public class ProfileInput
        {
            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            public string FullName { get; set; } = string.Empty;

            [Required, Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string PhoneNumber { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty; // chỉ hiển thị, không sửa
        }

        public class PasswordInput
        {
            public string? CurrentPassword { get; set; }

            [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
            public string? NewPassword { get; set; }

            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string? ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            Input = new ProfileInput
            {
                FullName    = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email       = user.Email
            };
            return Page();
        }

        // Cập nhật thông tin cá nhân
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName    = Input.FullName;
            user.PhoneNumber = Input.PhoneNumber;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToPage();
        }

        // Đổi mật khẩu
        public async Task<IActionResult> OnPostPasswordAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(PasswordChange.CurrentPassword) ||
                string.IsNullOrEmpty(PasswordChange.NewPassword))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin mật khẩu!";
                return RedirectToPage();
            }

            if (!BCrypt.Net.BCrypt.Verify(PasswordChange.CurrentPassword, user.PasswordHash))
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToPage();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordChange.NewPassword);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToPage();
        }
    }
}
