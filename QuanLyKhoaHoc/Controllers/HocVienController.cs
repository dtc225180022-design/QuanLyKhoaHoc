using Microsoft.AspNetCore.Mvc;

namespace QuanLyKhoaHoc.Controllers;

// Không cần [Authorize] server-side — auth được xử lý bởi JavaScript client-side
// (giống AdminController pattern). API endpoints vẫn có [Authorize] để bảo vệ dữ liệu.
public class HocVienController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult KetQua() => View();
    public IActionResult LichSuDangKy() => View();
    public IActionResult HoSo() => View();

    // Trang thanh toán: nhận dangKyId từ query string sau khi đăng ký khóa học
    public IActionResult ThanhToan(int id) => View((object)id);
}
