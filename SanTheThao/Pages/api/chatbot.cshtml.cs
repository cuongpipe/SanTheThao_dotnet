using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using SanTheThao.Services; // Nhúng namespace chứa ICourtService của bạn

namespace SanTheThao.Pages.api
{
    [IgnoreAntiforgeryToken]
    public class chatbotModel : PageModel
    {
        private readonly IConfiguration _configuration;
        // 1. Khai báo ICourtService để tương tác với DB
        private readonly ICourtService _courtService; 

        // 2. Inject các dịch vụ vào thông qua Constructor
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
                // BÚT PHÁP BỐC DATA TỪ DATABASE NÉM VÀO MỒM AI
                // =========================================================================
                
                // Lấy toàn bộ danh mục môn thể thao kèm danh sách sân thuộc môn đó
                var sportTypes = await _courtService.GetAllSportTypesAsync();

                var dataContext = new StringBuilder();
                dataContext.AppendLine("Dưới đây là danh sách thông tin sân thực tế trên hệ thống Sân Thể Thao:");
                dataContext.AppendLine("- Giờ mở cửa hệ thống: 5h00 đến 23h00 hằng ngày.");

                if (sportTypes != null && sportTypes.Count > 0)
                {
                    foreach (var sport in sportTypes)
                    {
                        dataContext.AppendLine($"\n* Môn thể thao: {sport.Name}"); 
                        
                        if (sport.Courts != null && sport.Courts.Count > 0)
                        {
                            // Đã bỏ qua bộ lọc IsActive, lấy toàn bộ danh sách sân được trả về
                            foreach (var court in sport.Courts)
                            {
                                // AI sẽ đọc được tên sân và giá tiền của toàn bộ các sân
                                dataContext.AppendLine($"  - Sân: {court.Name} | Giá: {court.PricePerHour:N0} VNĐ/giờ"); 
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

                // IN LOG RA TERMINAL ĐỂ KIỂM TRA CHUỖI TEXT CONTEXT
                Console.WriteLine($"[DEBUG DATABASE CONTEXT]:\n{dataContext}");

                // =========================================================================

                var url = "https://api.groq.com/openai/v1/chat/completions";

                // 3. Ném dữ liệu thô từ DB vào System Prompt của Llama
                var payload = new
                {
                    model = "llama-3.1-8b-instant", 
                    messages = new[]
                    {
                        new { 
                            role = "system", 
                            content = $"Bạn là trợ lý ảo thông minh của website Sân Thể Thao. Hãy sử dụng thông tin danh sách sân và giá tiền lấy từ Database dưới đây để tư vấn cho người dùng một cách chính xác, ngắn gọn và lịch sự bằng tiếng Việt. Nếu người dùng hỏi những thông tin ngoài dữ liệu được cung cấp, hãy bảo họ liên hệ hotline: 0901 234 567 để biết thêm chi tiết.\n\n" +
                            $"HƯỚNG DẪN NGƯỜI DÙNG ĐẶT SÂN:\n" +
                            $"- Nhắc nhở người dùng có thể đặt sân trực tuyến bằng cách nhấn vào mục 'Đặt sân' trên thanh điều hướng (Navbar/Header) của website.\n" +
                            $"- Hướng dẫn họ các bước đặt sân đơn giản bao gồm: 1. Chọn sân -> 2. Chọn khung giờ muốn đặt -> 3. Chọn phương thức thanh toán -> 4. Thêm ghi chú nếu cần -> 5. Nhấn 'Xác nhận đặt sân'.\n\n" +
                            $"DỮ LIỆU HỆ THỐNG:\n{dataContext}"
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