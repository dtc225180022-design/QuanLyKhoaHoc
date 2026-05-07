using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/hoc-vien")]
[Authorize]
public class HocVienApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public HocVienApiController(ApplicationDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _db.HocViens
            .Include(h => h.NguoiDung)
            .Include(h => h.DanhSachDangKy)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(h => h.NguoiDung.HoTen.Contains(search)
                                  || h.MaHocVien.Contains(search)
                                  || h.NguoiDung.Email.Contains(search));

        var list = await query.OrderByDescending(h => h.NgayDangKy).ToListAsync();
        return Ok(list.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var hv = await _db.HocViens
            .Include(h => h.NguoiDung)
            .Include(h => h.DanhSachDangKy).ThenInclude(d => d.KhoaHoc)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hv == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && hv.NguoiDungId != currentUserId)
            return Forbid();

        return Ok(MapToDto(hv));
    }

    [HttpGet("cua-toi")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hv = await _db.HocViens
            .Include(h => h.NguoiDung)
            .Include(h => h.DanhSachDangKy).ThenInclude(d => d.KhoaHoc)
            .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

        if (hv == null) return NotFound();
        return Ok(MapToDto(hv));
    }

    [HttpPut("cua-toi")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] CapNhatHocVienDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hv = await _db.HocViens.Include(h => h.NguoiDung)
            .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

        if (hv == null) return NotFound(new { message = "Không tìm thấy hồ sơ học viên" });

        hv.NguoiDung.HoTen = dto.HoTen;
        hv.NguoiDung.SoDienThoai = dto.SoDienThoai;
        hv.NgaySinh = dto.NgaySinh;
        hv.GioiTinh = dto.GioiTinh;
        hv.DiaChi = dto.DiaChi;
        hv.TrinhDoHienTai = dto.TrinhDoHienTai;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật hồ sơ thành công" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CapNhatHocVienDto dto)
    {
        var hv = await _db.HocViens.Include(h => h.NguoiDung).FirstOrDefaultAsync(h => h.Id == id);
        if (hv == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!User.IsInRole("Admin") && hv.NguoiDungId != currentUserId) return Forbid();

        hv.NguoiDung.HoTen = dto.HoTen;
        hv.NguoiDung.SoDienThoai = dto.SoDienThoai;
        hv.NgaySinh = dto.NgaySinh;
        hv.GioiTinh = dto.GioiTinh;
        hv.DiaChi = dto.DiaChi;
        hv.TrinhDoHienTai = dto.TrinhDoHienTai;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(hv));
    }

    [HttpGet("{id}/dang-ky")]
    public async Task<IActionResult> GetDangKy(int id)
    {
        var hv = await _db.HocViens
            .Include(h => h.NguoiDung)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (hv == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!User.IsInRole("Admin") && hv.NguoiDungId != currentUserId) return Forbid();

        var danhSach = await _db.DangKys
            .Include(d => d.KhoaHoc)
            .Include(d => d.Diem)
            .Where(d => d.HocVienId == id)
            .OrderByDescending(d => d.NgayDangKy)
            .ToListAsync();

        return Ok(danhSach.Select(d => new DangKyDto
        {
            Id = d.Id,
            TenHocVien = hv.NguoiDung?.HoTen ?? "",
            MaHocVien = hv.MaHocVien,
            TenKhoaHoc = d.KhoaHoc.TenKhoaHoc,
            MaKhoaHoc = d.KhoaHoc.MaKhoaHoc,
            NgayDangKy = d.NgayDangKy,
            TrangThai = d.TrangThai.ToString(),
            SoTienDaThanhToan = d.SoTienDaThanhToan,
            HocPhi = d.KhoaHoc.HocPhi,
            NgayThanhToan = d.NgayThanhToan,
            GhiChu = d.GhiChu
        }));
    }

    [HttpPost("dang-ky-khoa-hoc")]
    public async Task<IActionResult> DangKyKhoaHoc([FromBody] DangKyKhoaHocDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);
        if (hv == null) return BadRequest(new { message = "Không tìm thấy hồ sơ học viên" });

        var kh = await _db.KhoaHocs.Include(k => k.DanhSachDangKy)
            .FirstOrDefaultAsync(k => k.Id == dto.KhoaHocId);
        if (kh == null) return NotFound(new { message = "Khóa học không tồn tại" });

        if (kh.TrangThai != TrangThaiKhoaHoc.MoChuaHoc)
            return BadRequest(new { message = "Khóa học không còn nhận đăng ký" });

        var soHocVien = kh.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo);
        if (soHocVien >= kh.SoLuongToiDa)
            return BadRequest(new { message = "Khóa học đã đầy chỗ" });

        if (await _db.DangKys.AnyAsync(d => d.HocVienId == hv.Id && d.KhoaHocId == dto.KhoaHocId))
            return BadRequest(new { message = "Bạn đã đăng ký khóa học này rồi" });

        var dangKy = new DangKy
        {
            HocVienId = hv.Id,
            KhoaHocId = dto.KhoaHocId,
            GhiChu = dto.GhiChu
        };

        _db.DangKys.Add(dangKy);
        await _db.SaveChangesAsync();

        _db.Diems.Add(new Diem { DangKyId = dangKy.Id });
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Đăng ký thành công! Vui lòng chọn phương thức thanh toán.",
            dangKyId = dangKy.Id,
            redirectUrl = $"/HocVien/ThanhToan/{dangKy.Id}"
        });
    }

    private static HocVienDto MapToDto(HocVien h) => new()
    {
        Id = h.Id,
        NguoiDungId = h.NguoiDungId,
        MaHocVien = h.MaHocVien,
        HoTen = h.NguoiDung?.HoTen ?? "",
        Email = h.NguoiDung?.Email ?? "",
        SoDienThoai = h.NguoiDung?.SoDienThoai ?? "",
        AnhDaiDien = h.NguoiDung?.AnhDaiDien ?? "",
        NgaySinh = h.NgaySinh,
        GioiTinh = h.GioiTinh,
        DiaChi = h.DiaChi,
        TrinhDoHienTai = h.TrinhDoHienTai,
        NgayDangKy = h.NgayDangKy,
        SoKhoaHocDangHoc = h.DanhSachDangKy.Count(d => d.TrangThai == TrangThaiDangKy.DangHoc)
    };
}
