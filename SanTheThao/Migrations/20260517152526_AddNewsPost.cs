using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SanTheThao.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsPosts_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "NewsPosts",
                columns: new[] { "Id", "AuthorId", "Category", "Content", "CreatedAt", "IsPublished", "Slug", "Summary", "ThumbnailUrl", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Cầu lông", "Cầu lông là một trong những môn thể thao phổ biến nhất tại Việt Nam. Chơi cầu lông đều đặn giúp cải thiện sức bền tim mạch, tăng cường phản xạ, giảm stress và đốt cháy calories hiệu quả.", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "loi-ich-choi-cau-long", "Cầu lông không chỉ là môn thể thao vui vẻ mà còn mang lại vô số lợi ích sức khoẻ bạn chưa biết.", null, "5 lợi ích tuyệt vời của việc chơi cầu lông mỗi ngày" },
                    { 2, 1, "Pickleball", "Pickleball là sự kết hợp giữa tennis, bóng bàn và cầu lông. Môn này phù hợp mọi lứa tuổi, dễ học và rất thú vị. Năm 2025, Pickleball đã có mặt tại hầu hết các tỉnh thành lớn ở Việt Nam.", new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "pickleball-hot-2025", "Pickleball đang bùng nổ tại Việt Nam với hàng nghìn người chơi mới mỗi tháng. Tìm hiểu ngay!", null, "Pickleball — Môn thể thao hot nhất 2025 tại Việt Nam" },
                    { 3, 1, "Bóng đá", "Có 3 loại giày phổ biến: FG (sân cỏ tự nhiên), AG (sân cỏ nhân tạo) và TF (sân phủi). Tại Việt Nam hầu hết sân mini là cỏ nhân tạo nên chọn giày AG hoặc TF để tránh trơn trượt.", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "chon-giay-da-bong", "Giày đá bóng sai loại sân không chỉ ảnh hưởng hiệu suất mà còn dễ gây chấn thương.", null, "Hướng dẫn chọn giày đá bóng phù hợp với loại sân" },
                    { 4, 1, "Bóng rổ", "Bóng rổ 3x3 chỉ cần 1 rổ, 3 người mỗi đội và sân nhỏ hơn. Trận đấu kéo dài 10 phút hoặc đội nào đạt 21 điểm trước sẽ thắng. Format này rất phù hợp cho sân bóng rổ đô thị.", new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "bong-ro-3x3-xu-huong", "Bóng rổ 3x3 đang thu hút đông đảo giới trẻ Việt Nam nhờ format nhanh, kịch tính và dễ tổ chức.", null, "Bóng rổ 3x3 — Xu hướng mới cho giới trẻ Việt" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi");

            migrationBuilder.CreateIndex(
                name: "IX_NewsPosts_AuthorId",
                table: "NewsPosts",
                column: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsPosts");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$43uD9/LEK4I160H3wmYi7urqfovNxCADfQxW03.5WDaxu1zQSwSMS");
        }
    }
}
