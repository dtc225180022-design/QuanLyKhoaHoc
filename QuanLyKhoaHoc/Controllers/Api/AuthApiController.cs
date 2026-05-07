using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Services;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);
        if (!result)
            return BadRequest(new { message = "Email đã tồn tại trong hệ thống" });

        return Ok(new { message = "Đăng ký thành công" });
    }

    [Authorize]
    [HttpPost("doi-mat-khau")]
    public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.DoiMatKhauAsync(userId, dto);
        if (!result)
            return BadRequest(new { message = "Mật khẩu cũ không đúng" });

        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            HoTen = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            VaiTro = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
