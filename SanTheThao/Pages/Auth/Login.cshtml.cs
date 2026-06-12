using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.DTOs;
using SanTheThao.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;

namespace SanTheThao.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _auth;

        public LoginModel(IAuthService auth) => _auth = auth;

        [BindProperty]
        public LoginDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Index");
            return Page();
        }

        // ===== ĐĂNG NHẬP BẰNG EMAIL/PASSWORD TRUYỀN THỐNG =====
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _auth.LoginAsync(Input.Email, Input.Password);

            if (user == null)
            {
                ErrorMessage = "Email hoặc mật khẩu không đúng";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.FullName),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role),
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = Input.RememberMe }
            );

            return user.Role == "Admin"
                ? RedirectToPage("/Admin/Dashboard")
                : RedirectToPage("/Index");
        }



 // =========================================================================
        // XỬ LÝ ĐĂNG NHẬP EXTERNAL (GOOGLE / GITHUB)
        // =========================================================================

        // 1. Khi User bấm nút Google
        public IActionResult OnPostGoogleLogin()
        {
            var redirectUrl = Url.Page("./Login", pageHandler: "ExternalCallback");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // 2. Khi User bấm nút GitHub
        public IActionResult OnPostChallenge(string provider = "GitHub")
        {
            var redirectUrl = Url.Page("./Login", pageHandler: "ExternalCallback"); 
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        // 3. HÀM CALLBACK CHUNG CHO CẢ GOOGLE VÀ GITHUB - VẢ PHÁT ĂN NGAY
        public async Task<IActionResult> OnGetExternalCallbackAsync()
        {
            // Đọc thông tin từ Identity Provider gửi về (Dùng Cookie chứa thông tin login ngoài)
            // Nếu dự án dùng Identity thuần thì dùng IdentityConstants.ExternalScheme, 
            // Nếu không thì cứ AuthenticateAsync() trống để .NET tự dò Scheme ngoại.
            var result = await HttpContext.AuthenticateAsync();
            
            if (!result.Succeeded || result.Principal == null)
            {
                ErrorMessage = "Đăng nhập bằng tài khoản liên kết thất bại, vui lòng thử lại.";
                return RedirectToPage("./Login");
            }

            var principal = result.Principal;

            // Bóc tách Email thông minh (Ép bốc mọi ngóc ngách của Google & GitHub)
            var email = principal.FindFirstValue(ClaimTypes.Email) 
                        ?? principal.FindFirstValue("email");

            // Bóc tách Tên đầy đủ (GitHub có thể trả về 'name' hoặc username là 'login')
            var fullName = principal.FindFirstValue(ClaimTypes.Name) 
                           ?? principal.FindFirstValue("name")
                           ?? principal.FindFirstValue("login") 
                           ?? "User Liên Kết";

            if (string.IsNullOrEmpty(email))
            {
                ErrorMessage = "Không thể lấy thông tin Email từ tài khoản liên kết công khai của bạn.";
                return RedirectToPage("./Login");
            }

            // Gọi hàm xử lý bất tử của mày trong AuthService để kiểm tra hoặc lưu mới vào SQL Server
            var user = await _auth.GetOrCreateUserFromSocialLoginAsync(email, fullName);

            // Tạo danh sách quyền (Claims) đồng bộ 100% với hệ thống Sân Thể Thao
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.FullName),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role ?? "Customer"), // Tự động nhận Admin/Customer
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var mainPrincipal = new ClaimsPrincipal(identity);

            // Cấp Cookie chính thức cho ứng dụng hoạt động ổn định
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                mainPrincipal,
                new AuthenticationProperties { IsPersistent = true }
            );

            // Điều hướng chuẩn chỉnh theo phân quyền bốc lên từ Database
            return user.Role == "Admin"
                ? RedirectToPage("/Admin/Dashboard")
                : RedirectToPage("/Index");
        }
    }
}