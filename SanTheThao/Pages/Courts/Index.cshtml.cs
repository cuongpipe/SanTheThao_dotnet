using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.Models;
using SanTheThao.Services;

namespace SanTheThao.Pages.Courts
{
    public class IndexModel : PageModel
    {
        private readonly ICourtService _courtService;

        public IndexModel(ICourtService courtService) => _courtService = courtService;

        public List<SportType> SportTypes { get; set; } = new();
        public List<Court> Courts { get; set; } = new();
        public SportType? SelectedSport { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SportTypeId { get; set; }

        public async Task OnGetAsync()
        {
            SportTypes = await _courtService.GetAllSportTypesAsync();

            if (SportTypeId.HasValue)
            {
                Courts = await _courtService.GetCourtsBySportTypeAsync(SportTypeId.Value);
                SelectedSport = await _courtService.GetSportTypeByIdAsync(SportTypeId.Value);
            }
            else
            {
                // Mặc định hiện tất cả sân
                foreach (var sport in SportTypes)
                {
                    Courts.AddRange(await _courtService.GetCourtsBySportTypeAsync(sport.Id));
                }
            }
        }
    }
}