using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Services;
using Microsoft.AspNetCore.Authentication.Google;

using Microsoft.AspNetCore.Authentication.Facebook;


var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<PostReviewService>();




// ==================== AUTHENTICATION & GOOGLE LOGIN GỘP CHUNG ====================
builder.Services.AddAuthentication(options =>
{
    // Đặt Cookie làm Scheme mặc định để lưu phiên đăng nhập
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    
    // Nếu bạn muốn hễ user chưa đăng nhập mà vào trang bắt buộc là TỰ ĐỘNG đá sang Google, hãy bật dòng dưới:
    // options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Giữ nguyên các cấu hình Cookie cũ của bạn ở đây
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

})
.AddGitHub(options =>
{
    // Đọc thông tin từ file appsettings.json
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
    
    // Xin quyền bốc thêm Email công khai của tài khoản GitHub
    options.Scope.Add("user:email");
})
.AddFacebook(facebookOptions =>
{
    // Đọc thông tin từ file appsettings.json
    facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
    facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
    
    // Yêu cầu Facebook trả về thêm thông tin nếu cần (mặc định đã có tên và id)
    facebookOptions.Fields.Add("picture"); // Lấy thêm ảnh đại diện
});




builder.Services.AddAuthorization();
// =================================================================================




// ===== SERVICES =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourtService, CourtService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddRazorPages();









var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 2. Đảm bảo luồng Middleware có đủ 2 dòng này xếp đúng thứ tự:
app.UseAuthentication(); // Xác thực danh tính

app.UseAuthorization();  // Phân quyền




app.MapRazorPages();




app.Run();

// Định nghĩa Class nhận dữ liệu (đặt hẳn ở ngoài, dưới cùng file Program.cs luôn)
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}