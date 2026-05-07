using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;

namespace QuanLyKhoaHoc.Controllers;

public class KhoaHocController : Controller
{
    private readonly ApplicationDbContext _db;
    public KhoaHocController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? ngonNgu, string? trinhDo, string? search)
    {
        var query = _db.KhoaHocs
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .AsQueryable();

        if (!string.IsNullOrEmpty(ngonNgu)) query = query.Where(k => k.NgonNgu == ngonNgu);
        if (!string.IsNullOrEmpty(trinhDo)) query = query.Where(k => k.TrinhDo == trinhDo);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(k => k.TenKhoaHoc.Contains(search) || k.MaKhoaHoc.Contains(search) || k.MoTa.Contains(search));

        ViewBag.NgonNgu = ngonNgu;
        ViewBag.TrinhDo = trinhDo;
        ViewBag.Search = search;
        ViewBag.DanhSachNgonNgu = await _db.KhoaHocs.Select(k => k.NgonNgu).Distinct().ToListAsync();
        ViewBag.DanhSachTrinhDo = await _db.KhoaHocs.Select(k => k.TrinhDo).Distinct().ToListAsync();

        var list = await query.OrderByDescending(k => k.NgayBatDau).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var kh = await _db.KhoaHocs
            .Include(k => k.DanhSachLichHoc)
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kh == null) return NotFound();
        return View(kh);
    }
}
