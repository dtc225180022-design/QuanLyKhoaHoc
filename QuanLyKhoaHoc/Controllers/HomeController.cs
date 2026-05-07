using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var khoaHocMo = await _db.KhoaHocs
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Where(k => k.TrangThai == TrangThaiKhoaHoc.MoChuaHoc)
            .OrderBy(k => k.NgayBatDau)
            .Take(6)
            .ToListAsync();

        ViewBag.TongKhoaHoc = await _db.KhoaHocs.CountAsync();
        ViewBag.TongHocVien = await _db.HocViens.CountAsync();
        ViewBag.TongGiangVien = await _db.GiangViens.CountAsync();
        ViewBag.KhoaHocMo = khoaHocMo;

        var tongDiem = await _db.Diems.CountAsync(d => d.DaHoanThanh);
        var sodat = await _db.Diems.CountAsync(d => d.DaHoanThanh && d.DuDieuKienCapChungChi);
        ViewBag.TiLeDat = tongDiem > 0 ? (int)Math.Round(sodat * 100.0 / tongDiem) : 95;

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
