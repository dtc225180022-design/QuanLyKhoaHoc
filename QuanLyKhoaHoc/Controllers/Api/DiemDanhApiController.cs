using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/diem-danh")]
[Authorize(Roles = "GiangVien,Admin")]
public class DiemDanhApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DiemDanhApiController(ApplicationDbContext db) => _db = db;

    // Lấy danh sách điểm danh của 1 buổi học
    [HttpGet("buoi/{buoiHocId}")]
    public async Task<IActionResult> GetTheobuoi(int buoiHocId)
    {
        if (!await KiemTraQuyenAsync(buoiHocId)) return Forbid();

        var buoi = await _db.BuoiHocs
            .Include(b => b.KhoaHoc).ThenInclude(k => k.DanhSachDangKy).ThenInclude(d => d.HocVien).ThenInclude(h => h.NguoiDung)
            .Include(b => b.DanhSachDiemDanh)
            .FirstOrDefaultAsync(b => b.Id == buoiHocId);

        if (buoi == null) return NotFound();

        var kh = buoi.KhoaHoc;
        var dsDangKy = kh.DanhSachDangKy
            .Where(d => d.TrangThai == TrangThaiDangKy.DangHoc || d.TrangThai == TrangThaiDangKy.DaDuyet)
            .ToList();

        var result = dsDangKy.Select(dk => {
            var dd = buoi.DanhSachDiemDanh.FirstOrDefault(d => d.DangKyId == dk.Id);
            return new
            {
                DangKyId = dk.Id,
                HocVienId = dk.HocVienId,
                HoTen = dk.HocVien?.NguoiDung?.HoTen ?? "",
                MaHocVien = dk.HocVien?.MaHocVien ?? "",
                TrangThai = dd?.TrangThai.ToString() ?? "ChuaDiemDanh",
                GhiChu = dd?.GhiChu ?? "",
                DiemDanhId = dd?.Id
            };
        });

        return Ok(new
        {
            BuoiHocId = buoi.Id,
            SoBuoiThuTu = buoi.SoBuoiThuTu,
            NgayHoc = buoi.NgayHoc.ToString("dd/MM/yyyy"),
            PhongHoc = buoi.PhongHoc,
            HinhThuc = buoi.HinhThuc.ToString(),
            DaDienRa = buoi.DaDienRa,
            TongSoHocVien = dsDangKy.Count,
            DanhSach = result
        });
    }

    // Lưu/cập nhật điểm danh hàng loạt cho 1 buổi
    [HttpPost("buoi/{buoiHocId}")]
    public async Task<IActionResult> LuuDiemDanh(int buoiHocId, [FromBody] List<DiemDanhItemDto> danhSach)
    {
        if (!await KiemTraQuyenAsync(buoiHocId)) return Forbid();

        var buoi = await _db.BuoiHocs.FindAsync(buoiHocId);
        if (buoi == null) return NotFound();

        foreach (var item in danhSach)
        {
            var existing = await _db.DiemDanhs.FirstOrDefaultAsync(d => d.BuoiHocId == buoiHocId && d.DangKyId == item.DangKyId);
            if (existing == null)
            {
                _db.DiemDanhs.Add(new DiemDanh
                {
                    BuoiHocId = buoiHocId,
                    DangKyId = item.DangKyId,
                    TrangThai = item.TrangThai,
                    GhiChu = item.GhiChu,
                    NgayDiemDanh = DateTime.Now
                });
            }
            else
            {
                existing.TrangThai = item.TrangThai;
                existing.GhiChu = item.GhiChu;
                existing.NgayDiemDanh = DateTime.Now;
            }
        }

        // Đánh dấu buổi đã diễn ra
        buoi.DaDienRa = true;
        await _db.SaveChangesAsync();

        // Tự động cập nhật điểm chuyên cần
        await CapNhatDiemChuyenCanAsync(buoi.KhoaHocId);

        return Ok(new { message = "Lưu điểm danh thành công", soLuong = danhSach.Count });
    }

    // Thống kê chuyên cần cho 1 khóa
    [HttpGet("thong-ke/{khoaHocId}")]
    public async Task<IActionResult> ThongKeChuyenCan(int khoaHocId)
    {
        var kh = await _db.KhoaHocs
            .Include(k => k.DanhSachBuoiHoc).ThenInclude(b => b.DanhSachDiemDanh)
            .Include(k => k.DanhSachDangKy).ThenInclude(d => d.HocVien).ThenInclude(h => h.NguoiDung)
            .FirstOrDefaultAsync(k => k.Id == khoaHocId);
        if (kh == null) return NotFound();

        var tongBuoi = kh.DanhSachBuoiHoc.Count(b => b.DaDienRa);
        var gioi_han = (int)Math.Ceiling(kh.SoBuoiHoc * 0.2);

        var result = kh.DanhSachDangKy
            .Where(d => d.TrangThai == TrangThaiDangKy.DangHoc || d.TrangThai == TrangThaiDangKy.DaDuyet)
            .Select(dk => {
                var dsDiemDanh = kh.DanhSachBuoiHoc.Where(b => b.DaDienRa)
                    .SelectMany(b => b.DanhSachDiemDanh.Where(dd => dd.DangKyId == dk.Id))
                    .ToList();
                var soVang = dsDiemDanh.Count(dd => dd.TrangThai == TrangThaiDiemDanh.Vang);
                var soVangCoPhep = dsDiemDanh.Count(dd => dd.TrangThai == TrangThaiDiemDanh.VangCoPhep);
                var soCoMat = dsDiemDanh.Count(dd => dd.TrangThai == TrangThaiDiemDanh.CoMat || dd.TrangThai == TrangThaiDiemDanh.HocOnline);
                var phanTramDiHoc = tongBuoi > 0 ? (soCoMat * 100.0 / tongBuoi) : 100;
                var canh_bao = soVang > gioi_han;
                return new
                {
                    DangKyId = dk.Id,
                    HoTen = dk.HocVien?.NguoiDung?.HoTen ?? "",
                    MaHocVien = dk.HocVien?.MaHocVien ?? "",
                    SoCoMat = soCoMat,
                    SoVang = soVang,
                    SoVangCoPhep = soVangCoPhep,
                    TongBuoiDaDienRa = tongBuoi,
                    PhanTramDiHoc = Math.Round(phanTramDiHoc, 1),
                    CanhBao = canh_bao
                };
            });

        return Ok(new { TongBuoi = tongBuoi, GioiHanNghi = gioi_han, DanhSach = result });
    }

    private async Task CapNhatDiemChuyenCanAsync(int khoaHocId)
    {
        var kh = await _db.KhoaHocs
            .Include(k => k.DanhSachBuoiHoc).ThenInclude(b => b.DanhSachDiemDanh)
            .Include(k => k.DanhSachDangKy)
            .FirstOrDefaultAsync(k => k.Id == khoaHocId);
        if (kh == null) return;

        var tongBuoi = kh.DanhSachBuoiHoc.Count(b => b.DaDienRa);
        if (tongBuoi == 0) return;

        foreach (var dk in kh.DanhSachDangKy)
        {
            var soCoMat = kh.DanhSachBuoiHoc.Where(b => b.DaDienRa)
                .SelectMany(b => b.DanhSachDiemDanh.Where(d => d.DangKyId == dk.Id &&
                    (d.TrangThai == TrangThaiDiemDanh.CoMat || d.TrangThai == TrangThaiDiemDanh.HocOnline || d.TrangThai == TrangThaiDiemDanh.VangCoPhep)))
                .Count();

            var diemCC = (decimal)(soCoMat * 10.0 / tongBuoi);
            diemCC = Math.Min(10, Math.Round(diemCC, 2));

            var diem = await _db.Diems.FirstOrDefaultAsync(d => d.DangKyId == dk.Id);
            if (diem != null)
            {
                diem.DiemChuyenCan = diemCC;
                CapNhatTongKet(diem);
            }
        }
        await _db.SaveChangesAsync();
    }

    private static void CapNhatTongKet(Diem d)
    {
        if (d.DiemChuyenCan.HasValue && d.DiemGiuaKy.HasValue && d.DiemCuoiKy.HasValue)
        {
            // Thang điểm 0-100: CC(0-10)×10% + GK(0-10)×30% + CK(0-10)×60%
            // = CC×1 + GK×3 + CK×6 → max = 10+30+60 = 100
            d.DiemTongKet = Math.Round(d.DiemChuyenCan.Value * 1m + d.DiemGiuaKy.Value * 3m + d.DiemCuoiKy.Value * 6m, 2);
            d.DiemTrungBinh = d.DiemTongKet;
            d.XepLoai = d.DiemTongKet switch
            {
                >= 90 => "Xuất sắc",
                >= 80 => "Giỏi",
                >= 65 => "Khá",
                >= 50 => "Trung bình",
                _ => "Không đạt"
            };
            d.DuDieuKienCapChungChi = d.DiemTongKet >= 50;
            d.DaHoanThanh = true;
            d.NgayCapNhat = DateTime.Now;
        }
    }

    private async Task<bool> KiemTraQuyenAsync(int buoiHocId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Admin") return true;

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
        if (gv == null) return false;

        var buoi = await _db.BuoiHocs.FindAsync(buoiHocId);
        if (buoi == null) return false;

        return await _db.PhanCongs.AnyAsync(p => p.GiangVienId == gv.Id && p.KhoaHocId == buoi.KhoaHocId);
    }
}

public class DiemDanhItemDto
{
    public int DangKyId { get; set; }
    public TrangThaiDiemDanh TrangThai { get; set; }
    public string GhiChu { get; set; } = string.Empty;
}
