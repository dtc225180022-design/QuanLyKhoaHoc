using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;

namespace QuanLyKhoaHoc.Controllers;

public class TinTucController : Controller
{
    private readonly ApplicationDbContext _db;
    public TinTucController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? theLoai, string? search, int page = 1)
    {
        const int pageSize = 9;
        var query = _db.BaiViets
            .Where(b => b.DangHienThi)
            .AsQueryable();

        if (!string.IsNullOrEmpty(theLoai))
            query = query.Where(b => b.TheLoai == theLoai);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(b => b.TieuDe.Contains(search) || b.TomTat.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.NoiBat)
            .ThenByDescending(b => b.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TheLoai = theLoai;
        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total = total;
        ViewBag.DanhSachTheLoai = await _db.BaiViets
            .Where(b => b.DangHienThi)
            .Select(b => b.TheLoai)
            .Distinct()
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var baiViet = await _db.BaiViets
            .Include(b => b.NguoiTao)
            .FirstOrDefaultAsync(b => b.Id == id && b.DangHienThi);

        if (baiViet == null) return NotFound();

        // Tăng lượt xem
        baiViet.LuotXem++;
        await _db.SaveChangesAsync();

        // Bài viết liên quan
        var lienQuan = await _db.BaiViets
            .Where(b => b.DangHienThi && b.Id != id && b.TheLoai == baiViet.TheLoai)
            .OrderByDescending(b => b.NgayTao)
            .Take(3)
            .ToListAsync();

        ViewBag.LienQuan = lienQuan;
        return View(baiViet);
    }
}
