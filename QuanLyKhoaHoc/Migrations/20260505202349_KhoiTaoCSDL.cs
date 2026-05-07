using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoCSDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhoaHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoaHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenKhoaHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgonNgu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrinhDo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoBuoiHoc = table.Column<int>(type: "int", nullable: false),
                    ThoiLuongMoiBuoi = table.Column<int>(type: "int", nullable: false),
                    HocPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VaiTro = table.Column<int>(type: "int", nullable: false),
                    HoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LichHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhoaHocId = table.Column<int>(type: "int", nullable: false),
                    ThuTrongTuan = table.Column<int>(type: "int", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichHocs_KhoaHocs_KhoaHocId",
                        column: x => x.KhoaHocId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GiangViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    MaGiangVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChuyenNganh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BangCap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NamKinhNghiem = table.Column<int>(type: "int", nullable: false),
                    GioiThieu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiangViens_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HocViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    MaHocVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrinhDoHienTai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocViens_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiangVienId = table.Column<int>(type: "int", nullable: false),
                    KhoaHocId = table.Column<int>(type: "int", nullable: false),
                    NgayPhanCong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanCongs_GiangViens_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongs_KhoaHocs_KhoaHocId",
                        column: x => x.KhoaHocId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    KhoaHocId = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    SoTienDaThanhToan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKys_HocViens_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKys_KhoaHocs_KhoaHocId",
                        column: x => x.KhoaHocId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Diems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyId = table.Column<int>(type: "int", nullable: false),
                    DiemGiuaKy = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiemCuoiKy = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiemTrungBinh = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    XepLoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DaHoanThanh = table.Column<bool>(type: "bit", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diems_DangKys_DangKyId",
                        column: x => x.DangKyId,
                        principalTable: "DangKys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKys_HocVienId_KhoaHocId",
                table: "DangKys",
                columns: new[] { "HocVienId", "KhoaHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKys_KhoaHocId",
                table: "DangKys",
                column: "KhoaHocId");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_DangKyId",
                table: "Diems",
                column: "DangKyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_MaGiangVien",
                table: "GiangViens",
                column: "MaGiangVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_NguoiDungId",
                table: "GiangViens",
                column: "NguoiDungId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_MaHocVien",
                table: "HocViens",
                column: "MaHocVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_NguoiDungId",
                table: "HocViens",
                column: "NguoiDungId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHocs_MaKhoaHoc",
                table: "KhoaHocs",
                column: "MaKhoaHoc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_KhoaHocId",
                table: "LichHocs",
                column: "KhoaHocId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_Email",
                table: "NguoiDungs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongs_GiangVienId_KhoaHocId",
                table: "PhanCongs",
                columns: new[] { "GiangVienId", "KhoaHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongs_KhoaHocId",
                table: "PhanCongs",
                column: "KhoaHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diems");

            migrationBuilder.DropTable(
                name: "LichHocs");

            migrationBuilder.DropTable(
                name: "PhanCongs");

            migrationBuilder.DropTable(
                name: "DangKys");

            migrationBuilder.DropTable(
                name: "GiangViens");

            migrationBuilder.DropTable(
                name: "HocViens");

            migrationBuilder.DropTable(
                name: "KhoaHocs");

            migrationBuilder.DropTable(
                name: "NguoiDungs");
        }
    }
}
