using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.Models;
using SanTheThao.Services;

namespace SanTheThao.Pages.Courts
{
    public class DetailModel : PageModel
    {
        private readonly ICourtService _courtService;
        private readonly IBookingService _bookingService;
        private readonly ReviewService _reviewService;
        public List<Review> Reviews { get; set; } = new();
        [BindProperty]
        public Review NewReview { get; set; } = new();

        public DetailModel(ICourtService courtService, IBookingService bookingService, ReviewService reviewService)
        {
            _courtService = courtService;
            _bookingService = bookingService;
            _reviewService = reviewService;
        }

        public Court? Court { get; set; }
        public List<SanTheThao.Models.Booking> BookedSlots { get; set; } = new();

        // Khung giờ hoạt động: 6:00 - 22:00
        public List<TimeOnly> TimeSlots { get; set; } = Enumerable
            .Range(6, 16)
            .Select(h => new TimeOnly(h, 0))
            .ToList();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly SelectedDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (SelectedDate == default)
                SelectedDate = DateOnly.FromDateTime(DateTime.Today);

            Court = await _courtService.GetCourtByIdAsync(Id);
            if (Court == null) return NotFound();

            BookedSlots = await _bookingService.GetBookedSlotsAsync(Id, SelectedDate);
            Reviews = await _reviewService.GetByCourtIdAsync(Id);
            return Page();

        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _reviewService.AddAsync(NewReview);

            return RedirectToPage(new { id = NewReview.CourtId });
        }

        public bool IsSlotBooked(TimeOnly start)
        {
            return BookedSlots.Any(b =>
                b.StartTime <= start && b.EndTime > start);
        }

    }
}