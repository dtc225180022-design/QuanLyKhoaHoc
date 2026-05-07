using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/banner")]
public class BannerApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public BannerApiController(ApplicationDbContext db) => _db = db;

    // Public: lấy banner đang hiển thị cho trang chủ
    [HttpGet]
    public async Task<IActionResult> GetPublic()
    {
        var list = await _db.Banners
            .Where(b => b.DangHienThi)
            .OrderBy(b => b.ThuTu)
            .ToListAsync();
        return Ok(list);
    }

    // Admin: lấy tất cả banner
    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Banners.OrderBy(b => b.ThuTu).ToListAsync();
        return Ok(list);
    }

    // Admin: thêm banner
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BannerDto dto)
    {
        var banner = new Banner
        {
            TieuDe = dto.TieuDe,
            MoTa = dto.MoTa,
            HinhAnh = dto.HinhAnh,
            DuongDanLienKet = dto.DuongDanLienKet,
            ThuTu = dto.ThuTu,
            DangHienThi = dto.DangHienThi
        };
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
        return Ok(banner);
    }

    // Admin: sửa banner
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BannerDto dto)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();

        banner.TieuDe = dto.TieuDe;
        banner.MoTa = dto.MoTa;
        banner.HinhAnh = dto.HinhAnh;
        banner.DuongDanLienKet = dto.DuongDanLienKet;
        banner.ThuTu = dto.ThuTu;
        banner.DangHienThi = dto.DangHienThi;

        await _db.SaveChangesAsync();
        return Ok(banner);
    }

    // Admin: toggle hiển thị
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        banner.DangHienThi = !banner.DangHienThi;
        await _db.SaveChangesAsync();
        return Ok(new { id = banner.Id, dangHienThi = banner.DangHienThi });
    }

    // Admin: xóa banner
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Xóa banner thành công" });
    }
}

public class BannerDto
{
    [MaxLength(200)]
    public string TieuDe { get; set; } = string.Empty;
    [MaxLength(500)]
    public string MoTa { get; set; } = string.Empty;
    [MaxLength(500)]
    public string HinhAnh { get; set; } = string.Empty;
    [MaxLength(300)]
    public string DuongDanLienKet { get; set; } = string.Empty;
    public int ThuTu { get; set; } = 1;
    public bool DangHienThi { get; set; } = true;
}
