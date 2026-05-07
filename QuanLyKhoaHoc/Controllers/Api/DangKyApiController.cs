using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Models;
using System.Security.Claims;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/dang-ky")]
[Authorize]
public class DangKyApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DangKyApiController(ApplicationDbContext db) => _db = db;

    // Lịch sử đăng ký của học viên đang đăng nhập
    [HttpGet("cua-toi")]
    public async Task<IActionResult> GetCuaToi()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
        if (hv == null) return Ok(new List<object>());

        var list = await _db.DangKys
            .Include(d => d.KhoaHoc)
            .Where(d => d.HocVienId == hv.Id)
            .OrderByDescending(d => d.NgayDangKy)
            .ToListAsync();

        return Ok(list.Select(d => new
        {
            d.Id,
            TenKhoaHoc = d.KhoaHoc.TenKhoaHoc,
            MaKhoaHoc = d.KhoaHoc.MaKhoaHoc,
            NgonNgu = d.KhoaHoc.NgonNgu,
            HocPhi = d.KhoaHoc.HocPhi,
            NgayDangKy = d.NgayDangKy,
            TrangThai = d.TrangThai.ToString(),
            SoTienDaThanhToan = d.SoTienDaThanhToan,
            NgayThanhToan = d.NgayThanhToan,
            PhuongThucThanhToan = d.PhuongThucThanhToan,
            GhiChu = d.GhiChu
        }));
    }

    // Học viên chọn phương thức thanh toán sau khi đăng ký
    [HttpPut("{id}/chon-phuong-thuc")]
    public async Task<IActionResult> ChonPhuongThuc(int id, [FromBody] ChonPhuongThucDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
        if (hv == null) return Forbid();

        var dk = await _db.DangKys
            .Include(d => d.KhoaHoc)
            .FirstOrDefaultAsync(d => d.Id == id && d.HocVienId == hv.Id);
        if (dk == null) return NotFound(new { message = "Không tìm thấy đăng ký" });
        if (dk.TrangThai != TrangThaiDangKy.ChoDuyet)
            return BadRequest(new { message = "Đăng ký này không ở trạng thái chờ duyệt" });

        dk.PhuongThucThanhToan = dto.PhuongThuc;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Đã chọn phương thức thanh toán. Vui lòng chờ admin xác nhận.",
            dangKyId = dk.Id,
            tenKhoaHoc = dk.KhoaHoc.TenKhoaHoc,
            hocPhi = dk.KhoaHoc.HocPhi,
            phuongThuc = dk.PhuongThucThanhToan
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? khoaHocId, [FromQuery] string? trangThai)
    {
        var query = _db.DangKys
            .Include(d => d.HocVien).ThenInclude(h => h.NguoiDung)
            .Include(d => d.KhoaHoc)
            .AsQueryable();

        if (khoaHocId.HasValue)
            query = query.Where(d => d.KhoaHocId == khoaHocId);

        if (!string.IsNullOrEmpty(trangThai) && Enum.TryParse<TrangThaiDangKy>(trangThai, out var ts))
            query = query.Where(d => d.TrangThai == ts);

        var list = await query.OrderByDescending(d => d.NgayDangKy).ToListAsync();
        return Ok(list.Select(d => new DangKyDto
        {
            Id = d.Id,
            TenHocVien = d.HocVien.NguoiDung?.HoTen ?? "",
            MaHocVien = d.HocVien.MaHocVien,
            TenKhoaHoc = d.KhoaHoc.TenKhoaHoc,
            MaKhoaHoc = d.KhoaHoc.MaKhoaHoc,
            NgayDangKy = d.NgayDangKy,
            TrangThai = d.TrangThai.ToString(),
            SoTienDaThanhToan = d.SoTienDaThanhToan,
            HocPhi = d.KhoaHoc.HocPhi,
            NgayThanhToan = d.NgayThanhToan,
            PhuongThucThanhToan = d.PhuongThucThanhToan,
            GhiChu = d.GhiChu
        }));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/trang-thai")]
    public async Task<IActionResult> CapNhatTrangThai(int id, [FromBody] CapNhatTrangThaiDangKyDto dto)
    {
        var dk = await _db.DangKys.FindAsync(id);
        if (dk == null) return NotFound();

        dk.TrangThai = (TrangThaiDangKy)dto.TrangThai;
        if (!string.IsNullOrEmpty(dto.GhiChu))
            dk.GhiChu = dto.GhiChu;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/thanh-toan")]
    public async Task<IActionResult> XacNhanThanhToan(int id, [FromBody] XacNhanThanhToanDto dto)
    {
        var dk = await _db.DangKys.Include(d => d.KhoaHoc).FirstOrDefaultAsync(d => d.Id == id);
        if (dk == null) return NotFound();

        dk.SoTienDaThanhToan = dto.SoTien > 0 ? dto.SoTien : dk.KhoaHoc.HocPhi;
        dk.NgayThanhToan = DateTime.Now;
        if (!string.IsNullOrEmpty(dto.PhuongThuc))
            dk.PhuongThucThanhToan = dto.PhuongThuc;
        // Khi xác nhận thanh toán → chuyển sang Đang học
        if (dk.TrangThai == TrangThaiDangKy.ChoDuyet || dk.TrangThai == TrangThaiDangKy.DaDuyet)
            dk.TrangThai = TrangThaiDangKy.DangHoc;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Xác nhận thanh toán thành công, học viên chuyển sang Đang học" });
    }
}
