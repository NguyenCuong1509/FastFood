using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastFoodOnline.Migrations
{
    /// <inheritdoc />
    public partial class soluongmonan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoLuongTonKho",
                table: "MonAns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoLuongTonKho",
                table: "MonAns");
        }
    }
}
