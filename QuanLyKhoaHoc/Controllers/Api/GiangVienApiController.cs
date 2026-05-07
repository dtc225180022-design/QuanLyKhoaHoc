using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/giang-vien")]
public class GiangVienApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public GiangVienApiController(ApplicationDbContext db) => _db = db;

    [Authorize(Roles = "GiangVien")]
    [HttpGet("cua-toi")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gv = await _db.GiangViens
            .Include(g => g.NguoiDung)
            .Include(g => g.DanhSachPhanCong).ThenInclude(p => p.KhoaHoc)
            .FirstOrDefaultAsync(g => g.NguoiDungId == userId);

        if (gv == null) return NotFound(new { message = "Không tìm thấy hồ sơ giảng viên" });
        return Ok(MapToDto(gv));
    }

    [Authorize(Roles = "GiangVien")]
    [HttpPut("cua-toi")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] CapNhatGiangVienDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gv = await _db.GiangViens.Include(g => g.NguoiDung)
            .FirstOrDefaultAsync(g => g.NguoiDungId == userId);

        if (gv == null) return NotFound(new { message = "Không tìm thấy hồ sơ giảng viên" });

        gv.NguoiDung.HoTen = dto.HoTen;
        gv.NguoiDung.SoDienThoai = dto.SoDienThoai;
        gv.ChuyenNganh = dto.ChuyenNganh;
        gv.BangCap = dto.BangCap;
        gv.NamKinhNghiem = dto.NamKinhNghiem;
        gv.GioiThieu = dto.GioiThieu;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật hồ sơ thành công", data = MapToDto(gv) });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _db.GiangViens
            .Include(g => g.NguoiDung)
            .Include(g => g.DanhSachPhanCong)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(g => g.NguoiDung.HoTen.Contains(search)
                                  || g.ChuyenNganh.Contains(search)
                                  || g.MaGiangVien.Contains(search));

        var list = await query.Where(g => g.DangHoatDong).ToListAsync();
        return Ok(list.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gv = await _db.GiangViens
            .Include(g => g.NguoiDung)
            .Include(g => g.DanhSachPhanCong).ThenInclude(p => p.KhoaHoc)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gv == null) return NotFound();
        return Ok(MapToDto(gv));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaoGiangVienDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _db.NguoiDungs.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email đã tồn tại" });

        if (await _db.GiangViens.AnyAsync(g => g.MaGiangVien == dto.MaGiangVien))
            return BadRequest(new { message = "Mã giảng viên đã tồn tại" });

        var user = new NguoiDung
        {
            HoTen = dto.HoTen,
            Email = dto.Email,
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
            SoDienThoai = dto.SoDienThoai,
            VaiTro = VaiTro.GiangVien
        };
        _db.NguoiDungs.Add(user);
        await _db.SaveChangesAsync();

        var gv = new GiangVien
        {
            NguoiDungId = user.Id,
            MaGiangVien = dto.MaGiangVien,
            ChuyenNganh = dto.ChuyenNganh,
            BangCap = dto.BangCap,
            NamKinhNghiem = dto.NamKinhNghiem,
            GioiThieu = dto.GioiThieu
        };
        _db.GiangViens.Add(gv);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = gv.Id }, MapToDto(gv));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CapNhatGiangVienDto dto)
    {
        var gv = await _db.GiangViens.Include(g => g.NguoiDung).FirstOrDefaultAsync(g => g.Id == id);
        if (gv == null) return NotFound();

        gv.NguoiDung.HoTen = dto.HoTen;
        gv.NguoiDung.SoDienThoai = dto.SoDienThoai;
        gv.ChuyenNganh = dto.ChuyenNganh;
        gv.BangCap = dto.BangCap;
        gv.NamKinhNghiem = dto.NamKinhNghiem;
        gv.GioiThieu = dto.GioiThieu;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(gv));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("phan-cong")]
    public async Task<IActionResult> PhanCong([FromBody] TaoPhanCongDto dto)
    {
        if (await _db.PhanCongs.AnyAsync(p => p.GiangVienId == dto.GiangVienId && p.KhoaHocId == dto.KhoaHocId))
            return BadRequest(new { message = "Giảng viên đã được phân công khóa học này" });

        var pc = new PhanCong
        {
            GiangVienId = dto.GiangVienId,
            KhoaHocId = dto.KhoaHocId,
            GhiChu = dto.GhiChu
        };
        _db.PhanCongs.Add(pc);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Phân công thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("phan-cong/{id}")]
    public async Task<IActionResult> XoaPhanCong(int id)
    {
        var pc = await _db.PhanCongs.FindAsync(id);
        if (pc == null) return NotFound();
        _db.PhanCongs.Remove(pc);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Hủy phân công thành công" });
    }

    [HttpGet("{id}/phan-cong")]
    public async Task<IActionResult> GetPhanCong(int id)
    {
        var list = await _db.PhanCongs
            .Include(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Include(p => p.KhoaHoc)
            .Where(p => p.GiangVienId == id)
            .ToListAsync();

        return Ok(list.Select(p => new PhanCongDto
        {
            Id = p.Id,
            TenGiangVien = p.GiangVien.NguoiDung.HoTen,
            MaGiangVien = p.GiangVien.MaGiangVien,
            TenKhoaHoc = p.KhoaHoc.TenKhoaHoc,
            MaKhoaHoc = p.KhoaHoc.MaKhoaHoc,
            NgayPhanCong = p.NgayPhanCong,
            DangHoatDong = p.DangHoatDong
        }));
    }

    private static GiangVienDto MapToDto(GiangVien g) => new()
    {
        Id = g.Id,
        NguoiDungId = g.NguoiDungId,
        MaGiangVien = g.MaGiangVien,
        HoTen = g.NguoiDung?.HoTen ?? "",
        Email = g.NguoiDung?.Email ?? "",
        SoDienThoai = g.NguoiDung?.SoDienThoai ?? "",
        AnhDaiDien = g.NguoiDung?.AnhDaiDien ?? "",
        ChuyenNganh = g.ChuyenNganh,
        BangCap = g.BangCap,
        NamKinhNghiem = g.NamKinhNghiem,
        GioiThieu = g.GioiThieu,
        DangHoatDong = g.DangHoatDong,
        SoKhoaHocPhuTrach = g.DanhSachPhanCong.Count(p => p.DangHoatDong)
    };
}
