using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/bai-viet")]
public class BaiVietApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public BaiVietApiController(ApplicationDbContext db) => _db = db;

    // GET /api/bai-viet  — public (hiển thị + phân trang)
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? theLoai,
        [FromQuery] bool? noiBat,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var q = _db.BaiViets.AsQueryable();
        if (!User.IsInRole("Admin"))
            q = q.Where(b => b.DangHienThi);
        if (!string.IsNullOrEmpty(theLoai))
            q = q.Where(b => b.TheLoai == theLoai);
        if (noiBat.HasValue)
            q = q.Where(b => b.NoiBat == noiBat);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(b => b.NgayTao)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new
            {
                b.Id, b.TieuDe, b.TomTat, b.HinhAnh, b.TheLoai,
                b.TacGia, b.DangHienThi, b.NoiBat, b.LuotXem,
                b.NgayTao, b.NgayCapNhat,
                nguoiTao = b.NguoiTao != null ? b.NguoiTao.HoTen : null
            }).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // GET /api/bai-viet/all — admin list (no pagination)
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAdmin()
    {
        var items = await _db.BaiViets
            .Include(b => b.NguoiTao)
            .OrderByDescending(b => b.NgayTao)
            .Select(b => new
            {
                b.Id, b.TieuDe, b.TomTat, b.HinhAnh, b.TheLoai,
                b.TacGia, b.DangHienThi, b.NoiBat, b.LuotXem,
                b.NgayTao, b.NgayCapNhat,
                nguoiTao = b.NguoiTao != null ? b.NguoiTao.HoTen : null
            }).ToListAsync();

        return Ok(items);
    }

    // GET /api/bai-viet/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var b = await _db.BaiViets.Include(x => x.NguoiTao).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound();
        if (!b.DangHienThi && !User.IsInRole("Admin")) return NotFound();

        // Tăng lượt xem
        b.LuotXem++;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            b.Id, b.TieuDe, b.TomTat, b.NoiDung, b.HinhAnh, b.TheLoai,
            b.TacGia, b.DangHienThi, b.NoiBat, b.LuotXem,
            b.NgayTao, b.NgayCapNhat,
            nguoiTao = b.NguoiTao?.HoTen
        });
    }

    // POST /api/bai-viet
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] BaiVietDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var baiViet = new BaiViet
        {
            TieuDe = dto.TieuDe.Trim(),
            TomTat = dto.TomTat?.Trim() ?? string.Empty,
            NoiDung = dto.NoiDung?.Trim() ?? string.Empty,
            HinhAnh = dto.HinhAnh?.Trim() ?? string.Empty,
            TheLoai = dto.TheLoai?.Trim() ?? "Tin tức",
            TacGia = dto.TacGia?.Trim() ?? string.Empty,
            DangHienThi = dto.DangHienThi,
            NoiBat = dto.NoiBat,
            NguoiTaoId = userId,
            NgayTao = DateTime.Now
        };
        _db.BaiViets.Add(baiViet);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Thêm bài viết thành công!", id = baiViet.Id });
    }

    // PUT /api/bai-viet/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] BaiVietDto dto)
    {
        var baiViet = await _db.BaiViets.FindAsync(id);
        if (baiViet == null) return NotFound(new { message = "Không tìm thấy bài viết." });

        baiViet.TieuDe = dto.TieuDe.Trim();
        baiViet.TomTat = dto.TomTat?.Trim() ?? string.Empty;
        baiViet.NoiDung = dto.NoiDung?.Trim() ?? string.Empty;
        baiViet.HinhAnh = dto.HinhAnh?.Trim() ?? string.Empty;
        baiViet.TheLoai = dto.TheLoai?.Trim() ?? "Tin tức";
        baiViet.TacGia = dto.TacGia?.Trim() ?? string.Empty;
        baiViet.DangHienThi = dto.DangHienThi;
        baiViet.NoiBat = dto.NoiBat;
        baiViet.NgayCapNhat = DateTime.Now;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật bài viết thành công!" });
    }

    // PATCH /api/bai-viet/{id}/toggle
    [HttpPatch("{id:int}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Toggle(int id)
    {
        var b = await _db.BaiViets.FindAsync(id);
        if (b == null) return NotFound();
        b.DangHienThi = !b.DangHienThi;
        b.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(new { message = b.DangHienThi ? "Đã hiển thị" : "Đã ẩn", dangHienThi = b.DangHienThi });
    }

    // DELETE /api/bai-viet/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var b = await _db.BaiViets.FindAsync(id);
        if (b == null) return NotFound(new { message = "Không tìm thấy bài viết." });
        _db.BaiViets.Remove(b);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã xóa bài viết." });
    }
}

public record BaiVietDto(
    string TieuDe,
    string? TomTat,
    string? NoiDung,
    string? HinhAnh,
    string? TheLoai,
    string? TacGia,
    bool DangHienThi = true,
    bool NoiBat = false
);
