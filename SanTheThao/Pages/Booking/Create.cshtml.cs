using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.DTOs;
using SanTheThao.Models;
using SanTheThao.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;

namespace SanTheThao.Pages.Booking
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ICourtService _courtService;
        private readonly IBookingService _bookingService;

        public CreateModel(ICourtService courtService, IBookingService bookingService)
        {
            _courtService = courtService;
            _bookingService = bookingService;
        }

        public Court? Court { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CourtId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Date { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Start { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string End { get; set; } = string.Empty;

        [BindProperty]
        public string? Note { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; } = "Cash";

        public async Task<IActionResult> OnGetAsync()
        {
            Court = await _courtService.GetCourtByIdAsync(CourtId);
            if (Court == null) return NotFound();

            var startTime = TimeOnly.Parse(Start);
            var endTime   = TimeOnly.Parse(End);
            var hours     = (decimal)(endTime - startTime).TotalHours;
            TotalPrice    = Court.PricePerHour * hours;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Court = await _courtService.GetCourtByIdAsync(CourtId);
            if (Court == null) return NotFound();

            var bookingDate = DateOnly.Parse(Date);
            var startTime   = TimeOnly.Parse(Start);
            var endTime     = TimeOnly.Parse(End);

            var hours  = (decimal)(endTime - startTime).TotalHours;
            TotalPrice = Court.PricePerHour * hours;

            var available = await _bookingService.IsCourtAvailableAsync(CourtId, bookingDate, startTime, endTime);
            if (!available)
            {
                ErrorMessage = "Khung giờ này đã được đặt, vui lòng chọn giờ khác!";
                return Page();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var dto = new BookingDto
            {
                CourtId     = CourtId,
                UserId      = userId,
                BookingDate = bookingDate,
                StartTime   = startTime,
                EndTime     = endTime,
                Note        = Note
            };

            // 1. Tạo đơn đặt sân tạm thời
            var booking = await _bookingService.CreateBookingAsync(dto);

            // 2. LUỒNG XỬ LÝ MOMO V3 CHUẨN ĐÉT
            if (PaymentMethod == "Momo")
            {
                int bookingIdToDelete = booking.Id;

                try
                {
                    string endpoint    = "https://test-payment.momo.vn/v2/gateway/api/create"; 
                    // Thay thế cụm cũ bằng cụm chuẩn Sandbox V3 này:
                    string partnerCode = "MOMO"; 
                    string accessKey   = "F8BBA842ECF85"; 
                    string secretKey   = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
                    
                    string redirectUrl = "http://localhost:5167/Booking/Success";
                    string ipnUrl      = "http://localhost:5167/api/momocallback";
                    string requestType = "captureWallet"; 

                    // Sinh mã đơn ngẫu nhiên dùng GUID chống trùng lặp Sandbox
                    string randomGuid  = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
                    string orderId     = "BILL" + booking.Id + "X" + randomGuid;
                    string requestId   = Guid.NewGuid().ToString().Replace("-", ""); 
                    
                    // Để orderInfo ngắn gọn, thuần chữ và số để tránh lỗi Signature chênh lệch giữa Client và Server MoMo
                    string orderInfo   = "Thanh toan dat san " + booking.Id; 
                    string amount      = ((long)TotalPrice).ToString(); 
                    string extraData   = ""; 

                    // ĐỒNG BỘ CHUỖI THÔ TUYỆT ĐỐI THEO SPECS V3 (Sắp xếp tăng dần Alphabet từ a-z của khóa)
                    string rawData = $"accessKey={accessKey}" +
                                     $"&amount={amount}" +
                                     $"&extraData={extraData}" +
                                     $"&ipnUrl={ipnUrl}" +
                                     $"&orderId={orderId}" +
                                     $"&orderInfo={orderInfo}" +
                                     $"&partnerCode={partnerCode}" +
                                     $"&redirectUrl={redirectUrl}" +
                                     $"&requestId={requestId}" +
                                     $"&requestType={requestType}";

                    Console.WriteLine("RAW DATA:");
                    Console.WriteLine(rawData);

                    string signature = CreateSignature(rawData, secretKey);

                    Console.WriteLine("SIGNATURE:");
                    Console.WriteLine(signature);

                    // ĐÓNG GÓI ĐỐI TƯỢNG JSON ĐỒNG BỘ KHÍT TỪNG TÊN BIẾN VỚI RAW DATA
                    var requestData = new
                    {
                        partnerCode = partnerCode,
                        accessKey = accessKey,
                        requestId = requestId,
                        amount = amount,
                        orderId = orderId,
                        orderInfo = orderInfo,
                        redirectUrl = redirectUrl,
                        ipnUrl = ipnUrl,
                        lang = "vi",
                        extraData = extraData,
                        requestType = requestType,
                        signature = signature
                    };

                    using var client = new HttpClient();
                    var response = await client.PostAsJsonAsync(endpoint, requestData);
                    var rawResponse = await response.Content.ReadAsStringAsync();
                    
                    Console.WriteLine("========== MOMO RESPONSE ==========");
                    Console.WriteLine(rawResponse);
                    Console.WriteLine("==================================");

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(rawResponse);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("resultCode", out var resultCodeProp))
                        {
                            int resultCode = resultCodeProp.GetInt32();

                            if (resultCode != 0)
                            {
                                string message = root.TryGetProperty("message", out var msgProp)
                                    ? msgProp.GetString() ?? "MoMo Error"
                                    : "MoMo Error";

                                await _bookingService.CancelBookingAsync(bookingIdToDelete, userId);

                                ErrorMessage = $"MoMo Error ({resultCode}): {message}";
                                return Page();
                            }
                        }

                        if (root.TryGetProperty("payUrl", out var payUrlProp))
                        {
                            string payUrl = payUrlProp.GetString() ?? "";

                            if (!string.IsNullOrWhiteSpace(payUrl))
                            {
                                return Redirect(payUrl);
                            }
                        }
                    }

                    await _bookingService.CancelBookingAsync(bookingIdToDelete, userId);
                    ErrorMessage = "Cổng MoMo từ chối giao dịch hoặc dữ liệu không hợp lệ. Đã giải phóng sân!";
                    return Page();
                }
                catch (Exception ex)
                {
                    await _bookingService.CancelBookingAsync(bookingIdToDelete, userId);
                    Console.WriteLine($"[Momo Crash]: {ex.Message}");
                    ErrorMessage = "Hệ thống MoMo đang bận. Vui lòng thử lại sau.";
                    return Page();
                }
            }

            return RedirectToPage("/Booking/Success", new { id = booking.Id });
        }

        // THUẬT TOÁN BĂM CHUẨN HMAC-SHA256
        private static string CreateSignature(string rawData, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(rawData);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // "x2" đảm bảo in ra chữ thường (lowercase hex)
                }
                return sb.ToString();
            }
        }
    }
}