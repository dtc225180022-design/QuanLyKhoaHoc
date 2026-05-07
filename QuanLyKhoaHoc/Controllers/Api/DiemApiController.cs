using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/diem")]
[Authorize]
public class DiemApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DiemApiController(ApplicationDbContext db) => _db = db;

    // GV/Admin: lấy điểm theo khóa học
    [Authorize(Roles = "Admin,GiangVien")]
    [HttpGet("khoa-hoc/{khoaHocId}")]
    public async Task<IActionResult> GetByKhoaHoc(int khoaHocId)
    {
        if (!await KiemTraQuyenGiangVien(khoaHocId)) return Forbid();

        var list = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.HocVien).ThenInclude(h => h.NguoiDung)
            .Include(d => d.DangKy).ThenInclude(dk => dk.KhoaHoc)
            .Where(d => d.DangKy.KhoaHocId == khoaHocId && d.DangKy.TrangThai != TrangThaiDangKy.HuyBo)
            .OrderBy(d => d.DangKy.HocVien.NguoiDung.HoTen)
            .ToListAsync();

        return Ok(list.Select(MapToDto));
    }

    // GV/Admin: nhập điểm giữa kỳ và cuối kỳ
    [Authorize(Roles = "Admin,GiangVien")]
    [HttpPut("{id}")]
    public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatDiemDto dto)
    {
        var diem = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.KhoaHoc)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (diem == null) return NotFound();

        if (!await KiemTraQuyenGiangVien(diem.DangKy.KhoaHocId)) return Forbid();

        diem.DiemGiuaKy  = dto.DiemGiuaKy;
        diem.DiemCuoiKy  = dto.DiemCuoiKy;
        diem.NhanXet     = dto.NhanXet;
        diem.NgayCapNhat = DateTime.Now;

        TinhTongKet(diem);

        await _db.SaveChangesAsync();
        return Ok(MapToDto(diem));
    }

    // GV/Admin: danh sách lớp phụ trách
    [Authorize(Roles = "Admin,GiangVien")]
    [HttpGet("lop-cua-toi")]
    public async Task<IActionResult> GetLopCuaToi()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vaiTro = User.FindFirstValue(ClaimTypes.Role);

        IQueryable<PhanCong> query = _db.PhanCongs
            .Include(p => p.KhoaHoc).ThenInclude(k => k.DanhSachDangKy)
            .Include(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Where(p => p.DangHoatDong);

        if (vaiTro == "GiangVien")
        {
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
            if (gv == null) return Ok(new List<object>());
            query = query.Where(p => p.GiangVienId == gv.Id);
        }

        var list = await query.ToListAsync();
        return Ok(list.Select(p => new
        {
            khoaHocId    = p.KhoaHocId,
            tenKhoaHoc   = p.KhoaHoc.TenKhoaHoc,
            maKhoaHoc    = p.KhoaHoc.MaKhoaHoc,
            ngonNgu      = p.KhoaHoc.NgonNgu,
            trinhDo      = p.KhoaHoc.TrinhDo,
            trangThai    = p.KhoaHoc.TrangThai.ToString(),
            soHocVien    = p.KhoaHoc.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo),
            ngayBatDau   = p.KhoaHoc.NgayBatDau,
            ngayKetThuc  = p.KhoaHoc.NgayKetThuc
        }));
    }

    // Học viên: xem điểm của mình (đầy đủ)
    [HttpGet("cua-toi")]
    public async Task<IActionResult> GetMyDiem()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
        if (hv == null) return Ok(new List<object>());

        var list = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.KhoaHoc)
                .ThenInclude(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Include(d => d.DangKy).ThenInclude(dk => dk.KhoaHoc).ThenInclude(k => k.DanhSachBuoiHoc)
            .Where(d => d.DangKy.HocVienId == hv.Id)
            .OrderByDescending(d => d.DangKy.NgayDangKy)
            .ToListAsync();

        // Lấy điểm danh tổng hợp
        var allDiemDanh = await _db.DiemDanhs
            .Include(dd => dd.BuoiHoc)
            .Where(dd => dd.DangKy.HocVienId == hv.Id)
            .ToListAsync();

        return Ok(list.Select(d => {
            var kh = d.DangKy?.KhoaHoc;
            var tongBuoi = kh?.DanhSachBuoiHoc?.Count(b => b.DaDienRa) ?? 0;
            var ddList = allDiemDanh.Where(dd => dd.DangKyId == d.DangKyId).ToList();
            var soCoMat = ddList.Count(dd => dd.TrangThai == TrangThaiDiemDanh.CoMat || dd.TrangThai == TrangThaiDiemDanh.HocOnline || dd.TrangThai == TrangThaiDiemDanh.VangCoPhep);
            var soVang = ddList.Count(dd => dd.TrangThai == TrangThaiDiemDanh.Vang);
            return new
            {
                id = d.Id, dangKyId = d.DangKyId,
                khoaHocId = d.DangKy?.KhoaHocId,
                tenKhoaHoc = kh?.TenKhoaHoc, maKhoaHoc = kh?.MaKhoaHoc,
                ngonNgu = kh?.NgonNgu, trinhDo = kh?.TrinhDo,
                tenGiangVien = kh?.DanhSachPhanCong?.FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen ?? "—",
                trangThaiDangKy = d.DangKy?.TrangThai.ToString(),
                diemChuyenCan = d.DiemChuyenCan, diemGiuaKy = d.DiemGiuaKy,
                diemCuoiKy = d.DiemCuoiKy, diemTongKet = d.DiemTongKet,
                diemTrungBinh = d.DiemTrungBinh, xepLoai = d.XepLoai,
                daHoanThanh = d.DaHoanThanh, duDieuKienCapChungChi = d.DuDieuKienCapChungChi,
                nhanXet = d.NhanXet, ngayCapNhat = d.NgayCapNhat,
                ngayBatDau = kh?.NgayBatDau, ngayKetThuc = kh?.NgayKetThuc,
                tongBuoiHoc = kh?.SoBuoiHoc ?? 0, tongBuoiDaDienRa = tongBuoi,
                soCoMat, soVang,
                phanTramDiHoc = tongBuoi > 0 ? Math.Round(soCoMat * 100.0 / tongBuoi, 1) : 100.0
            };
        }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("tong-ket")]
    public async Task<IActionResult> TongKet()
    {
        var stats = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.KhoaHoc)
            .Where(d => d.DaHoanThanh)
            .GroupBy(d => d.DangKy.KhoaHoc.TenKhoaHoc)
            .Select(g => new { TenKhoaHoc = g.Key, SoHocVien = g.Count(), DiemTrungBinhLop = g.Average(d => d.DiemTrungBinh) })
            .ToListAsync();
        return Ok(stats);
    }

    // Helper
    private static void TinhTongKet(Diem d)
    {
        if (d.DiemGiuaKy.HasValue && d.DiemCuoiKy.HasValue)
        {
            // Thang điểm 0-100: CC(0-10)×1 + GK(0-10)×3 + CK(0-10)×6 → max 100
            var cc = d.DiemChuyenCan ?? 10m;
            d.DiemTongKet = Math.Round(cc * 1m + d.DiemGiuaKy.Value * 3m + d.DiemCuoiKy.Value * 6m, 2);
            d.DiemTrungBinh = d.DiemTongKet;
            d.XepLoai = d.DiemTongKet switch
            {
                >= 90m => "Xuất sắc",
                >= 80m => "Giỏi",
                >= 65m => "Khá",
                >= 50m => "Trung bình",
                _ => "Không đạt"
            };
            d.DuDieuKienCapChungChi = d.DiemTongKet >= 50m;
            d.DaHoanThanh = true;
            d.NgayCapNhat = DateTime.Now;
        }
    }

    private async Task<bool> KiemTraQuyenGiangVien(int khoaHocId)
    {
        var vaiTro = User.FindFirstValue(ClaimTypes.Role);
        if (vaiTro == "Admin") return true;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
        if (gv == null) return false;
        return await _db.PhanCongs.AnyAsync(p => p.GiangVienId == gv.Id && p.KhoaHocId == khoaHocId && p.DangHoatDong);
    }

    private static DiemDto MapToDto(Diem d) => new()
    {
        Id = d.Id, DangKyId = d.DangKyId,
        TenHocVien = d.DangKy?.HocVien?.NguoiDung?.HoTen ?? "",
        MaHocVien  = d.DangKy?.HocVien?.MaHocVien ?? "",
        TenKhoaHoc = d.DangKy?.KhoaHoc?.TenKhoaHoc ?? "",
        DiemChuyenCan = d.DiemChuyenCan,
        DiemGiuaKy = d.DiemGiuaKy, DiemCuoiKy = d.DiemCuoiKy,
        DiemTrungBinh = d.DiemTrungBinh, XepLoai = d.XepLoai,
        DaHoanThanh = d.DaHoanThanh, NhanXet = d.NhanXet, NgayCapNhat = d.NgayCapNhat
    };
}
