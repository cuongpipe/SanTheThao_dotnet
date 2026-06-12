using Microsoft.EntityFrameworkCore;
using SanTheThao.Data;
using SanTheThao.Models;

namespace SanTheThao.Services
{
    public class PostReviewService
    {
        private readonly AppDbContext _context;

        public PostReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostReview>> GetByPostIdAsync(int postId)
        {
            return await _context.PostReviews
                .Where(r => r.PostId == postId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }

        public async Task AddAsync(PostReview review)
        {
            _context.PostReviews.Add(review);
            await _context.SaveChangesAsync();
        }
    }
}