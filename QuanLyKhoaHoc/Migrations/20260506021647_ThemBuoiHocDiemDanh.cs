using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc.Migrations
{
    /// <inheritdoc />
    public partial class ThemBuoiHocDiemDanh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhongHoc",
                table: "LichHocs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "HinhThuc",
                table: "LichHocs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkMeetMacDinh",
                table: "LichHocs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "XepLoai",
                table: "Diems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<decimal>(
                name: "DiemChuyenCan",
                table: "Diems",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiemTongKet",
                table: "Diems",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DuDieuKienCapChungChi",
                table: "Diems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BuoiHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhoaHocId = table.Column<int>(type: "int", nullable: false),
                    LichHocId = table.Column<int>(type: "int", nullable: true),
                    NgayHoc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LinkMeet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HinhThuc = table.Column<int>(type: "int", nullable: false),
                    SoBuoiThuTu = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DaDienRa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuoiHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuoiHocs_KhoaHocs_KhoaHocId",
                        column: x => x.KhoaHocId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuoiHocs_LichHocs_LichHocId",
                        column: x => x.LichHocId,
                        principalTable: "LichHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DiemDanhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuoiHocId = table.Column<int>(type: "int", nullable: false),
                    DangKyId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NgayDiemDanh = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemDanhs_BuoiHocs_BuoiHocId",
                        column: x => x.BuoiHocId,
                        principalTable: "BuoiHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiemDanhs_DangKys_DangKyId",
                        column: x => x.DangKyId,
                        principalTable: "DangKys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHocs_KhoaHocId",
                table: "BuoiHocs",
                column: "KhoaHocId");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHocs_LichHocId",
                table: "BuoiHocs",
                column: "LichHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhs_BuoiHocId_DangKyId",
                table: "DiemDanhs",
                columns: new[] { "BuoiHocId", "DangKyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhs_DangKyId",
                table: "DiemDanhs",
                column: "DangKyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemDanhs");

            migrationBuilder.DropTable(
                name: "BuoiHocs");

            migrationBuilder.DropColumn(
                name: "HinhThuc",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "LinkMeetMacDinh",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "DiemChuyenCan",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "DiemTongKet",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "DuDieuKienCapChungChi",
                table: "Diems");

            migrationBuilder.AlterColumn<string>(
                name: "PhongHoc",
                table: "LichHocs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "XepLoai",
                table: "Diems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
