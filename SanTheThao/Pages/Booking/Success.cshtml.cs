using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.Data;
using SanTheThao.Models;
using SanTheThao.Services;

namespace SanTheThao.Pages.Booking
{
    [Authorize]
    public class SuccessModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly AppDbContext _db;

        public SuccessModel(IBookingService bookingService, AppDbContext db)
        {
            _bookingService = bookingService;
            _db = db;
        }

        public Models.Booking? Booking { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            string orderId = Request.Query["orderId"].ToString(); 
            string resultCode = Request.Query["resultCode"].ToString(); 
            int bookingId = 0;

            if (!string.IsNullOrEmpty(orderId) && orderId.StartsWith("BILL"))
            {
                var cleanStr = orderId.Replace("BILL", "");
                var parts = cleanStr.Split('X');
                if (parts.Length > 0)
                {
                    int.TryParse(parts[0], out bookingId);
                }

                if (resultCode == "0" && bookingId > 0)
                {
                    var liveBooking = await _db.Bookings.FindAsync(bookingId);
                    if (liveBooking != null && liveBooking.Status == BookingStatus.Pending)
                    {
                        liveBooking.Status = BookingStatus.Confirmed; 
                        await _db.SaveChangesAsync(); 
                    }
                }
                else if (resultCode != "0" && bookingId > 0)
                {
                    var trashBooking = await _db.Bookings.FindAsync(bookingId);
                    if (trashBooking != null)
                    {
                        _db.Bookings.Remove(trashBooking); 
                        await _db.SaveChangesAsync(); 
                    }
                    ViewData["PayErrorMessage"] = "Giao dịch qua ví MoMo thất bại hoặc bạn đã hủy thanh toán. Khung giờ đã được giải phóng!";
                    return Page();
                }
            }
            else
            {
                bookingId = id ?? 0;
                if (bookingId == 0)
                {
                    string idStr = Request.Query["id"].ToString();
                    int.TryParse(idStr, out bookingId);
                }
            }

            if (bookingId == 0) return NotFound();

            Booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (Booking == null) return NotFound();

            return Page();
        }
    }
}