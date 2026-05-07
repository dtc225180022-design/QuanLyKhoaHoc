using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc.Migrations
{
    /// <inheritdoc />
    public partial class AddBaiVietAndAnhDaiDien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhDaiDien",
                table: "NguoiDungs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BaiViets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TomTat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TheLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TacGia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DangHienThi = table.Column<bool>(type: "bit", nullable: false),
                    NoiBat = table.Column<bool>(type: "bit", nullable: false),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiViets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaiViets_NguoiDungs_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiViets_NguoiTaoId",
                table: "BaiViets",
                column: "NguoiTaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiViets");

            migrationBuilder.DropColumn(
                name: "AnhDaiDien",
                table: "NguoiDungs");
        }
    }
}
