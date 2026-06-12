using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SanTheThao.Models;
using SanTheThao.Services;

namespace SanTheThao.Pages.Courts
{
    public class SearchModel : PageModel
    {
        private readonly ICourtService _courtService;

        public SearchModel(ICourtService courtService)
        {
            _courtService = courtService;
        }

        public List<Court> Courts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public async Task OnGetAsync()
        {
            var sportTypes = await _courtService.GetAllSportTypesAsync();

            // Lấy tất cả sân (giống Index của bạn)
            foreach (var sport in sportTypes)
            {
                var list = await _courtService.GetCourtsBySportTypeAsync(sport.Id);
                Courts.AddRange(list);
            }

            // 🔥 LỌC THEO TÊN (đúng với code bạn)
            if (!string.IsNullOrEmpty(Search))
            {
                Courts = Courts
                    .Where(c => c.Name != null &&
                                c.Name.ToLower().Contains(Search.ToLower()))
                    .ToList();
            }
        }
    }
}