using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;

namespace QuanLyKhoaHoc.Controllers;

// Không cần [Authorize] server-side — auth được xử lý bởi JavaScript client-side.
// API endpoints vẫn có [Authorize] để bảo vệ dữ liệu.
public class GiangVienController : Controller
{
    private readonly ApplicationDbContext _db;
    public GiangVienController(ApplicationDbContext db) => _db = db;

    // Trang danh sách giảng viên (public)
    public async Task<IActionResult> Index()
    {
        var list = await _db.GiangViens
            .Include(g => g.NguoiDung)
            .Include(g => g.DanhSachPhanCong).ThenInclude(p => p.KhoaHoc)
            .Where(g => g.DangHoatDong)
            .OrderBy(g => g.NguoiDung.HoTen)
            .ToListAsync();
        return View(list);
    }

    // Dashboard riêng cho giảng viên
    public IActionResult Dashboard() => View();

    // Hồ sơ cá nhân giảng viên
    public IActionResult HoSo() => View();
}
