using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/lich-hoc")]
public class LichHocApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public LichHocApiController(ApplicationDbContext db) => _db = db;

    // Toàn bộ lịch học (admin, public)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? khoaHocId)
    {
        var query = _db.LichHocs
            .Include(l => l.KhoaHoc)
                .ThenInclude(k => k.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                        .ThenInclude(g => g.NguoiDung)
            .AsQueryable();

        if (khoaHocId.HasValue)
            query = query.Where(l => l.KhoaHocId == khoaHocId);

        var list = await query
            .Where(l => l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.DangHoc
                     || l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.MoChuaHoc)
            .ToListAsync();

        return Ok(list.Select(l => MapLichHoc(l)));
    }

    // Lịch học của học viên đang đăng nhập
    [Authorize]
    [HttpGet("cua-toi")]
    public async Task<IActionResult> GetMyLich()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vaiTro = User.FindFirstValue(ClaimTypes.Role);

        if (vaiTro == "Admin")
        {
            // Admin xem tất cả lịch học đang hoạt động
            var allLich = await _db.LichHocs
                .Include(l => l.KhoaHoc)
                    .ThenInclude(k => k.DanhSachPhanCong)
                        .ThenInclude(p => p.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                .Where(l => l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.DangHoc
                         || l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.MoChuaHoc)
                .ToListAsync();

            return Ok(allLich.Select(l => MapLichHoc(l)));
        }
        else if (vaiTro == "GiangVien")
        {
            // Giảng viên xem lịch dạy
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
            if (gv == null) return Ok(new List<object>());

            var lichDay = await _db.LichHocs
                .Include(l => l.KhoaHoc)
                    .ThenInclude(k => k.DanhSachPhanCong)
                        .ThenInclude(p => p.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                .Where(l => l.KhoaHoc.DanhSachPhanCong.Any(p => p.GiangVienId == gv.Id && p.DangHoatDong)
                         && (l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.DangHoc
                          || l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.MoChuaHoc))
                .ToListAsync();

            return Ok(lichDay.Select(l => MapLichHoc(l)));
        }
        else
        {
            // Học viên xem lịch học
            var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            if (hv == null) return Ok(new List<object>());

            var lichHoc = await _db.LichHocs
                .Include(l => l.KhoaHoc)
                    .ThenInclude(k => k.DanhSachDangKy)
                .Include(l => l.KhoaHoc)
                    .ThenInclude(k => k.DanhSachPhanCong)
                        .ThenInclude(p => p.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                .Where(l => l.KhoaHoc.DanhSachDangKy.Any(d => d.HocVienId == hv.Id
                         && d.TrangThai != TrangThaiDangKy.HuyBo)
                         && (l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.DangHoc
                          || l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.MoChuaHoc
                          || l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.DaKetThuc))
                .ToListAsync();

            return Ok(lichHoc.Select(l => MapLichHoc(l)));
        }
    }

    // Lịch học của giảng viên cụ thể
    [HttpGet("giang-vien/{giangVienId}")]
    public async Task<IActionResult> GetLichGiangVien(int giangVienId)
    {
        var list = await _db.LichHocs
            .Include(l => l.KhoaHoc)
                .ThenInclude(k => k.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                        .ThenInclude(g => g.NguoiDung)
            .Where(l => l.KhoaHoc.DanhSachPhanCong.Any(p => p.GiangVienId == giangVienId && p.DangHoatDong)
                     && (l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.DangHoc
                      || l.KhoaHoc.TrangThai == TrangThaiKhoaHoc.MoChuaHoc))
            .ToListAsync();

        return Ok(list.Select(l => MapLichHoc(l)));
    }

    private static object MapLichHoc(LichHoc l) => new
    {
        id = l.Id,
        khoaHocId = l.KhoaHocId,
        tenKhoaHoc = l.KhoaHoc?.TenKhoaHoc,
        maKhoaHoc = l.KhoaHoc?.MaKhoaHoc,
        ngonNgu = l.KhoaHoc?.NgonNgu,
        trinhDo = l.KhoaHoc?.TrinhDo,
        thu = (int)l.ThuTrongTuan,
        tenThu = l.ThuTrongTuan switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            DayOfWeek.Sunday => "Chủ nhật",
            _ => ""
        },
        gioBatDau = l.GioBatDau.ToString(@"hh\:mm"),
        gioKetThuc = l.GioKetThuc.ToString(@"hh\:mm"),
        phongHoc = l.PhongHoc,
        hinhThuc = l.HinhThuc.ToString(),
        linkMeet = l.LinkMeetMacDinh,
        ghiChu = l.GhiChu,
        giangVien = l.KhoaHoc?.DanhSachPhanCong
            .FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen ?? "Chưa phân công",
        ngayBatDau = l.KhoaHoc?.NgayBatDau,
        ngayKetThuc = l.KhoaHoc?.NgayKetThuc,
        soBuoiHoc = l.KhoaHoc?.SoBuoiHoc ?? 0,
        mauSac = GetColor(l.KhoaHoc?.NgonNgu)
    };

    private static string GetColor(string? ngonNgu) => ngonNgu switch
    {
        "Tiếng Anh" => "#0d6efd",
        "Tiếng Nhật" => "#dc3545",
        "Tiếng Hàn" => "#fd7e14",
        "Tiếng Trung" => "#198754",
        "Tiếng Pháp" => "#6f42c1",
        _ => "#6c757d"
    };
}
