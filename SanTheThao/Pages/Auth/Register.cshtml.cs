using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.DTOs;
using SanTheThao.Services;

namespace SanTheThao.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _auth;

        public RegisterModel(IAuthService auth) => _auth = auth;

        [BindProperty]
        public RegisterDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var (success, message) = await _auth.RegisterAsync(Input);

            if (!success)
            {
                ErrorMessage = message;
                return Page();
            }

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
