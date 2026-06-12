using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using SanTheThao.Services; // Nhúng namespace chứa ICourtService của mày

namespace SanTheThao.Pages.api
{
    [IgnoreAntiforgeryToken]
    public class chatbotModel : PageModel
    {
        private readonly IConfiguration _configuration;
        // 1. Khai báo ICourtService của mày
        private readonly ICourtService _courtService; 

        // 2. Inject ICourtService vào thông qua Constructor mặc định
        public chatbotModel(IConfiguration configuration, ICourtService courtService)
        {
            _configuration = configuration;
            _courtService = courtService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
            {
                return new BadRequestObjectResult(new { error = "Tin nhắn trống" });
            }

            try
            {
                var groqApiKey = _configuration["Groq:ApiKey"];
                if (string.IsNullOrEmpty(groqApiKey))
                {
                    return new JsonResult(new { reply = "Bot: Chưa cấu hình Groq API Key trong appsettings.json!" });
                }

                // =========================================================================
                // BÚT PHÁP BỐC DATA TỪ ICOURTSERVICE NÉM VÀO MỒM AI
                // =========================================================================
                
                // Gọi hàm lấy toàn bộ môn thể thao đang active kèm các sân tương ứng bên trong
                var sportTypes = await _courtService.GetAllSportTypesAsync();

                var dataContext = new StringBuilder();
                dataContext.AppendLine("Dưới đây là danh sách thông tin sân thực tế đang hoạt động trên hệ thống Sân Thể Thao:");
                dataContext.AppendLine("- Giờ mở cửa hệ thống: 5h00 đến 23h00 hằng ngày.");

                if (sportTypes != null && sportTypes.Count > 0)
                {
                    foreach (var sport in sportTypes)
                    {
                        dataContext.AppendLine($"\n* Môn thể thao: {sport.Name}"); // Giả sử thuộc tính tên môn là Name hoặc TenSport
                        
                        if (sport.Courts != null && sport.Courts.Count > 0)
                        {
                            foreach (var court in sport.Courts.Where(c => c.IsActive))
                            {
                                // Duyệt qua từng sân trong DB để nối chuỗi text làm Context cho AI đọc
                                dataContext.AppendLine($"  - Sân: {court.Name} (ID: {court.Id})"); 
                            }
                        }
                        else
                        {
                            dataContext.AppendLine("  (Hiện tại chưa có sân nào thuộc bộ môn này)");
                        }
                    }
                }
                else
                {
                    dataContext.AppendLine("- Hiện tại hệ thống đang bảo trì danh sách sân.");
                }

                // IN LOG RA TERMINAL ĐỂ MÀY XEM CỤM DATA TỪ DB NÓ BIẾN THÀNH TEXT NHƯ THẾ NÀO
                Console.WriteLine($"[DEBUG DATABASE CONTEXT]:\n{dataContext}");

                // =========================================================================

                var url = "https://api.groq.com/openai/v1/chat/completions";

                // 3. Ném cục dataContext (Chuỗi chữ bốc từ DB) vào system content
                var payload = new
                {
                    model = "llama-3.1-8b-instant", 
                    messages = new[]
                    {
                        new { 
                            role = "system", 
                            content = $"Bạn là trợ lý ảo thông minh của website Sân Thể Thao. Hãy sử dụng thông tin danh sách sân thực tế lấy từ Database dưới đây để tư vấn cho người dùng một cách chính xác, ngắn gọn và lịch sự bằng tiếng Việt. Nếu người dùng hỏi những thông tin ngoài dữ liệu được cung cấp, hãy bảo họ liên hệ hotline để biết thêm chi tiết.\n\nDỮ LIỆU HỆ THỐNG:\n{dataContext}" 
                        },
                        new { role = "user", content = request.Message }
                    }
                };

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", groqApiKey);

                var response = await client.PostAsJsonAsync(url, payload);

                if (response.IsSuccessStatusCode)
                {
                    var rawJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(rawJson);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("message", out var messageElement) &&
                            messageElement.TryGetProperty("content", out var contentElement))
                        {
                            string botReply = contentElement.GetString() ?? string.Empty;

                            if (!string.IsNullOrEmpty(botReply))
                            {
                                return new JsonResult(new { reply = botReply });
                            }
                        }
                    }
                    
                    return new JsonResult(new { reply = "Bot: Groq trả về nội dung rỗng." });
                }
                else
                {
                    var errContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Groq API Error Log]: {errContent}");
                    return new JsonResult(new { reply = $"Bot: Lỗi kết nối API Groq (Mã: {response.StatusCode})" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend Crash Log]: {ex.Message}");
                return new JsonResult(new { reply = $"Bot: Hệ thống gặp sự cố ngầm ({ex.Message})" });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}