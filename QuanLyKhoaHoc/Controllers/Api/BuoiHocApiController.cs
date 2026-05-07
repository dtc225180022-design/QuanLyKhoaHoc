using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/buoi-hoc")]
public class BuoiHocApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public BuoiHocApiController(ApplicationDbContext db) => _db = db;

    // ─── Học viên: buổi học hôm nay/sắp tới của mình ───────────────────
    [HttpGet("sap-toi")]
    [Authorize]
    public async Task<IActionResult> GetSapToi()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role   = User.FindFirst(ClaimTypes.Role)?.Value;
        var today  = DateTime.Today;
        var until  = today.AddDays(14);

        IQueryable<BuoiHoc> query = _db.BuoiHocs
            .Include(b => b.KhoaHoc)
                .ThenInclude(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Where(b => b.NgayHoc >= today && b.NgayHoc <= until);

        if (role == "User")
        {
            var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            if (hv == null) return Ok(new List<object>());
            var khoaHocIds = await _db.DangKys
                .Where(d => d.HocVienId == hv.Id && (d.TrangThai == TrangThaiDangKy.DangHoc || d.TrangThai == TrangThaiDangKy.DaDuyet))
                .Select(d => d.KhoaHocId).ToListAsync();
            query = query.Where(b => khoaHocIds.Contains(b.KhoaHocId));
        }
        else if (role == "GiangVien")
        {
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
            if (gv == null) return Ok(new List<object>());
            var khoaHocIds = await _db.PhanCongs.Where(p => p.GiangVienId == gv.Id && p.DangHoatDong).Select(p => p.KhoaHocId).ToListAsync();
            query = query.Where(b => khoaHocIds.Contains(b.KhoaHocId));
        }

        var list = await query.OrderBy(b => b.NgayHoc).ThenBy(b => b.GioBatDau).Take(10).ToListAsync();
        return Ok(list.Select(b => MapDto(b)));
    }

    // ─── Giảng viên / Admin: danh sách buổi của 1 khóa ─────────────────
    [HttpGet("theo-khoa-hoc/{khoaHocId}")]
    [Authorize]
    public async Task<IActionResult> GetTheoKhoaHoc(int khoaHocId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role   = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "GiangVien")
        {
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
            if (gv == null || !await _db.PhanCongs.AnyAsync(p => p.GiangVienId == gv.Id && p.KhoaHocId == khoaHocId))
                return Forbid();
        }

        var list = await _db.BuoiHocs
            .Where(b => b.KhoaHocId == khoaHocId)
            .OrderBy(b => b.SoBuoiThuTu)
            .ToListAsync();

        return Ok(list.Select(MapDto));
    }

    // ─── Cập nhật link Meet (GiangVien/Admin) ────────────────────────────
    [HttpPatch("{id}/link-meet")]
    [Authorize(Roles = "GiangVien,Admin")]
    public async Task<IActionResult> CapNhatLinkMeet(int id, [FromBody] string linkMeet)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role   = User.FindFirst(ClaimTypes.Role)?.Value;
        var buoi   = await _db.BuoiHocs.FindAsync(id);
        if (buoi == null) return NotFound();

        if (role == "GiangVien")
        {
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
            if (gv == null || !await _db.PhanCongs.AnyAsync(p => p.GiangVienId == gv.Id && p.KhoaHocId == buoi.KhoaHocId))
                return Forbid();
        }

        buoi.LinkMeet = linkMeet;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật link Meet thành công", linkMeet });
    }

    // ─── HocVien: lịch sử buổi học + điểm danh của mình ────────────────
    [HttpGet("cua-toi/{khoaHocId}")]
    [Authorize]
    public async Task<IActionResult> GetCuaToi(int khoaHocId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role   = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "User")
        {
            var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            if (hv == null) return Forbid();

            var dk = await _db.DangKys.FirstOrDefaultAsync(d => d.HocVienId == hv.Id
                && d.KhoaHocId == khoaHocId && d.TrangThai != TrangThaiDangKy.HuyBo);
            if (dk == null) return Forbid();

            var buois = await _db.BuoiHocs
                .Where(b => b.KhoaHocId == khoaHocId)
                .OrderBy(b => b.SoBuoiThuTu).ToListAsync();

            var diemDanhs = await _db.DiemDanhs
                .Where(d => d.DangKyId == dk.Id).ToListAsync();

            return Ok(buois.Select(b => new
            {
                b.Id, b.SoBuoiThuTu,
                NgayHoc    = b.NgayHoc.ToString("dd/MM/yyyy"),
                NgayHocRaw = b.NgayHoc,
                GioBatDau  = b.GioBatDau.ToString(@"hh\:mm"),
                GioKetThuc = b.GioKetThuc.ToString(@"hh\:mm"),
                b.PhongHoc, b.LinkMeet,
                HinhThuc   = b.HinhThuc.ToString(),
                b.DaDienRa,
                TrangThaiDiemDanh = diemDanhs.FirstOrDefault(dd => dd.BuoiHocId == b.Id)?.TrangThai.ToString() ?? "ChuaDiemDanh"
            }));
        }

        // GiangVien / Admin — same as theo-khoa-hoc
        var list = await _db.BuoiHocs
            .Where(b => b.KhoaHocId == khoaHocId)
            .OrderBy(b => b.SoBuoiThuTu).ToListAsync();
        return Ok(list.Select(MapDto));
    }

    // ─── Tạo buổi học (GiangVien/Admin) ─────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "GiangVien,Admin")]
    public async Task<IActionResult> TaoBuoiHoc([FromBody] TaoBuoiHocDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role   = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "GiangVien")
        {
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.NguoiDungId == userId);
            if (gv == null || !await _db.PhanCongs.AnyAsync(p => p.GiangVienId == gv.Id && p.KhoaHocId == dto.KhoaHocId))
                return Forbid();
        }

        var soThu = await _db.BuoiHocs.CountAsync(b => b.KhoaHocId == dto.KhoaHocId) + 1;
        var buoi = new BuoiHoc
        {
            KhoaHocId = dto.KhoaHocId,
            NgayHoc   = dto.NgayHoc,
            GioBatDau = dto.GioBatDau,
            GioKetThuc = dto.GioKetThuc,
            PhongHoc  = dto.PhongHoc,
            HinhThuc  = dto.HinhThuc,
            LinkMeet  = dto.LinkMeet,
            GhiChu    = dto.GhiChu,
            SoBuoiThuTu = soThu
        };
        _db.BuoiHocs.Add(buoi);
        await _db.SaveChangesAsync();
        return Ok(MapDto(buoi));
    }

    private static object MapDto(BuoiHoc b) => new
    {
        b.Id, b.KhoaHocId, b.SoBuoiThuTu,
        TenKhoaHoc = b.KhoaHoc?.TenKhoaHoc ?? "",
        NgayHoc    = b.NgayHoc.ToString("dd/MM/yyyy"),
        NgayHocRaw = b.NgayHoc,
        GioBatDau  = b.GioBatDau.ToString(@"hh\:mm"),
        GioKetThuc = b.GioKetThuc.ToString(@"hh\:mm"),
        b.PhongHoc, b.LinkMeet,
        HinhThuc   = b.HinhThuc.ToString(),
        b.GhiChu, b.DaDienRa,
        GiangVien  = b.KhoaHoc?.DanhSachPhanCong?.FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen ?? ""
    };
}

public class TaoBuoiHocDto
{
    public int KhoaHocId { get; set; }
    public DateTime NgayHoc { get; set; }
    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }
    public string PhongHoc { get; set; } = string.Empty;
    public HinhThucHoc HinhThuc { get; set; } = HinhThucHoc.Offline;
    public string LinkMeet { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
}
