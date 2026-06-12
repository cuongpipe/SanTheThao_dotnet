using Microsoft.EntityFrameworkCore;
using SanTheThao.Models;

namespace SanTheThao.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<SportType> SportTypes { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<NewsPost> NewsPosts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PostReview> PostReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision
            modelBuilder.Entity<Court>()
                .Property(c => c.PricePerHour)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // ===== SEED DATA =====

            // Sport Types
            modelBuilder.Entity<SportType>().HasData(
                new SportType { Id = 1, Name = "Bóng đá", Icon = "⚽", Description = "Sân bóng đá mini 5v5, 7v7" },
                new SportType { Id = 2, Name = "Cầu lông", Icon = "🏸", Description = "Sân cầu lông tiêu chuẩn" },
                new SportType { Id = 3, Name = "Bóng chuyền", Icon = "🏐", Description = "Sân bóng chuyền trong nhà" },
                new SportType { Id = 4, Name = "Bóng rổ", Icon = "🏀", Description = "Sân bóng rổ 3v3 và 5v5" },
                new SportType { Id = 5, Name = "Pickleball", Icon = "🎾", Description = "Sân Pickleball tiêu chuẩn" }
            );

            // Courts
            modelBuilder.Entity<Court>().HasData(
                // Bóng đá
                new Court { Id = 1, Name = "Sân Bóng Đá A1", SportTypeId = 1, PricePerHour = 200000 },
                new Court { Id = 2, Name = "Sân Bóng Đá A2", SportTypeId = 1, PricePerHour = 200000 },
                new Court { Id = 3, Name = "Sân Bóng Đá B1", SportTypeId = 1, PricePerHour = 250000 },
                // Cầu lông
                new Court { Id = 4, Name = "Sân Cầu Lông 1", SportTypeId = 2, PricePerHour = 80000 },
                new Court { Id = 5, Name = "Sân Cầu Lông 2", SportTypeId = 2, PricePerHour = 80000 },
                // Bóng chuyền
                new Court { Id = 6, Name = "Sân Bóng Chuyền 1", SportTypeId = 3, PricePerHour = 150000 },
                // Bóng rổ
                new Court { Id = 7, Name = "Sân Bóng Rổ 1", SportTypeId = 4, PricePerHour = 120000 },
                new Court { Id = 8, Name = "Sân Bóng Rổ 2", SportTypeId = 4, PricePerHour = 120000 },
                // Pickleball
                new Court { Id = 9, Name = "Sân Pickleball 1", SportTypeId = 5, PricePerHour = 100000 },
                new Court { Id = 10, Name = "Sân Pickleball 2", SportTypeId = 5, PricePerHour = 100000 }
            );

            // Admin mặc định (password: Admin@123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin",
                    Email = "admin@santhethao.com",
                    PhoneNumber = "0900000000",
                    PasswordHash = "$2a$11$43uD9/LEK4I160H3wmYi7urqfovNxCADfQxW03.5WDaxu1zQSwSMS",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );

            // Bài viết mẫu
            modelBuilder.Entity<NewsPost>().HasData(
                new NewsPost
                {
                    Id = 1,
                    AuthorId = 1,
                    Category = "Cầu lông",
                    IsPublished = true,
                    CreatedAt = new DateTime(2025, 1, 10),
                    Title = "5 lợi ích tuyệt vời của việc chơi cầu lông mỗi ngày",
                    Slug = "loi-ich-choi-cau-long",
                    Summary = "Cầu lông không chỉ là môn thể thao vui vẻ mà còn mang lại vô số lợi ích sức khoẻ bạn chưa biết.",
                    Content = "Cầu lông là một trong những môn thể thao phổ biến nhất tại Việt Nam. Chơi cầu lông đều đặn giúp cải thiện sức bền tim mạch, tăng cường phản xạ, giảm stress và đốt cháy calories hiệu quả."
                },
                new NewsPost
                {
                    Id = 2,
                    AuthorId = 1,
                    Category = "Pickleball",
                    IsPublished = true,
                    CreatedAt = new DateTime(2025, 2, 5),
                    Title = "Pickleball — Môn thể thao hot nhất 2025 tại Việt Nam",
                    Slug = "pickleball-hot-2025",
                    Summary = "Pickleball đang bùng nổ tại Việt Nam với hàng nghìn người chơi mới mỗi tháng. Tìm hiểu ngay!",
                    Content = "Pickleball là sự kết hợp giữa tennis, bóng bàn và cầu lông. Môn này phù hợp mọi lứa tuổi, dễ học và rất thú vị. Năm 2025, Pickleball đã có mặt tại hầu hết các tỉnh thành lớn ở Việt Nam."
                },
                new NewsPost
                {
                    Id = 3,
                    AuthorId = 1,
                    Category = "Bóng đá",
                    IsPublished = true,
                    CreatedAt = new DateTime(2025, 3, 1),
                    Title = "Hướng dẫn chọn giày đá bóng phù hợp với loại sân",
                    Slug = "chon-giay-da-bong",
                    Summary = "Giày đá bóng sai loại sân không chỉ ảnh hưởng hiệu suất mà còn dễ gây chấn thương.",
                    Content = "Có 3 loại giày phổ biến: FG (sân cỏ tự nhiên), AG (sân cỏ nhân tạo) và TF (sân phủi). Tại Việt Nam hầu hết sân mini là cỏ nhân tạo nên chọn giày AG hoặc TF để tránh trơn trượt."
                },
                new NewsPost
                {
                    Id = 4,
                    AuthorId = 1,
                    Category = "Bóng rổ",
                    IsPublished = true,
                    CreatedAt = new DateTime(2025, 4, 15),
                    Title = "Bóng rổ 3x3 — Xu hướng mới cho giới trẻ Việt",
                    Slug = "bong-ro-3x3-xu-huong",
                    Summary = "Bóng rổ 3x3 đang thu hút đông đảo giới trẻ Việt Nam nhờ format nhanh, kịch tính và dễ tổ chức.",
                    Content = "Bóng rổ 3x3 chỉ cần 1 rổ, 3 người mỗi đội và sân nhỏ hơn. Trận đấu kéo dài 10 phút hoặc đội nào đạt 21 điểm trước sẽ thắng. Format này rất phù hợp cho sân bóng rổ đô thị."
                }
            );
        }
    }
}