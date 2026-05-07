using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/upload")]
[Authorize]
public class UploadApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public UploadApiController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // POST /api/upload/avatar           — upload cho bản thân
    // POST /api/upload/avatar/{userId}  — upload cho người khác (Admin only)
    [HttpPost("avatar")]
    [HttpPost("avatar/{userId:int}")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, int? userId)
    {
        var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var myRole = User.FindFirstValue(ClaimTypes.Role);

        int targetId = userId ?? myId;
        if (targetId != myId && myRole != "Admin")
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file ảnh." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            return BadRequest(new { message = "Chỉ hỗ trợ ảnh JPG, PNG, GIF, WebP." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Ảnh không được vượt quá 5MB." });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var fileName = $"{targetId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/avatars/{fileName}";

        var nguoiDung = await _db.NguoiDungs.FindAsync(targetId);
        if (nguoiDung == null) return NotFound(new { message = "Không tìm thấy người dùng." });

        // Xóa ảnh cũ nếu là file local
        if (!string.IsNullOrEmpty(nguoiDung.AnhDaiDien)
            && nguoiDung.AnhDaiDien.StartsWith("/uploads/"))
        {
            var oldPath = Path.Combine(_env.WebRootPath, nguoiDung.AnhDaiDien.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        nguoiDung.AnhDaiDien = relativePath;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Tải ảnh lên thành công!", anhDaiDien = relativePath });
    }

    // GET /api/upload/avatar/{userId}
    [HttpGet("avatar/{userId:int}")]
    public async Task<IActionResult> GetAvatar(int userId)
    {
        var nd = await _db.NguoiDungs.FindAsync(userId);
        if (nd == null) return NotFound();
        return Ok(new { anhDaiDien = nd.AnhDaiDien });
    }
}
