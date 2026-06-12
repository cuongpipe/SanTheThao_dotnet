using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Services
{
    public interface ICourtService
    {
        Task<List<SportType>> GetAllSportTypesAsync();
        Task<SportType?> GetSportTypeByIdAsync(int id);
        Task<List<Court>> GetCourtsBySportTypeAsync(int sportTypeId);
        Task<Court?> GetCourtByIdAsync(int id);
    }

    public class CourtService : ICourtService
    {
        private readonly AppDbContext _db;

        public CourtService(AppDbContext db) => _db = db;

        public async Task<List<SportType>> GetAllSportTypesAsync()
            => await _db.SportTypes
                .Where(s => s.IsActive)
                .Include(s => s.Courts)
                .ToListAsync();

        public async Task<SportType?> GetSportTypeByIdAsync(int id)
            => await _db.SportTypes
                .Include(s => s.Courts.Where(c => c.IsActive))
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<Court>> GetCourtsBySportTypeAsync(int sportTypeId)
            => await _db.Courts
                .Where(c => c.SportTypeId == sportTypeId && c.IsActive)
                .Include(c => c.SportType)
                .ToListAsync();

        public async Task<Court?> GetCourtByIdAsync(int id)
            => await _db.Courts
                .Include(c => c.SportType)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }
}