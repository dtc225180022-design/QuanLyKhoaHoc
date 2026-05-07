using Microsoft.AspNetCore.Mvc;

namespace QuanLyKhoaHoc.Controllers;

public class AuthController : Controller
{
    public IActionResult DangNhap() => View();
    public IActionResult DangKy() => View();
}
