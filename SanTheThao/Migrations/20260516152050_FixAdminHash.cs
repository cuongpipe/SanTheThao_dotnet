using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SanTheThao.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$43uD9/LEK4I160H3wmYi7urqfovNxCADfQxW03.5WDaxu1zQSwSMS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$zu5V9QqrV7bXMKbSe/TnFOIjuJRZdQ3qGl.Y2cUQiGJHDFJOt37FS");
        }
    }
}
