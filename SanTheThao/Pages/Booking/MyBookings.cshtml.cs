using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.Models;
using SanTheThao.Services;
using System.Security.Claims;

namespace SanTheThao.Pages.Booking
{
    [Authorize]
    public class MyBookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public MyBookingsModel(IBookingService bookingService) => _bookingService = bookingService;

        public List<SanTheThao.Models.Booking> Bookings { get; set; } = new();
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Bookings = await _bookingService.GetUserBookingsAsync(userId);
        }

        public async Task<IActionResult> OnPostCancelAsync(int bookingId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _bookingService.CancelBookingAsync(bookingId, userId);

            TempData["Message"] = success ? "Hủy đặt sân thành công!" : "Không thể hủy đơn này!";
            TempData["IsError"] = !success;

            return RedirectToPage();
        }
    }
}
