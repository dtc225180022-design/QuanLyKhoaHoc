using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Services;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/ai")]
public class AIApiController : ControllerBase
{
    private readonly IAIRecommendService _aiService;
    private readonly ApplicationDbContext _db;

    public AIApiController(IAIRecommendService aiService, ApplicationDbContext db)
    {
        _aiService = aiService;
        _db = db;
    }

    [Authorize]
    [HttpGet("goi-y-cua-toi")]
    public async Task<IActionResult> GoiYChoToi()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var hv = await _db.HocViens.FirstOrDefaultAsync(h => h.NguoiDungId == userId);

        if (hv == null)
        {
            // Không phải học viên → trả về gợi ý phổ biến
            var popular = await _aiService.GoiYPhoBienAsync(4);
            return Ok(popular);
        }

        var result = await _aiService.GoiYKhoaHocAsync(hv.Id);
        if (!result.Any())
        {
            var popular = await _aiService.GoiYPhoBienAsync(4);
            return Ok(popular);
        }
        return Ok(result);
    }

    [HttpGet("goi-y")]
    public async Task<IActionResult> GoiYTheoTrinhDo(
        [FromQuery] string? trinhDo = "",
        [FromQuery] string? ngonNgu = "")
    {
        var result = await _aiService.GoiYTheoTrinhDoAsync(trinhDo ?? "", ngonNgu ?? "");
        return Ok(result);
    }

    [HttpGet("pho-bien")]
    public async Task<IActionResult> PhoBien([FromQuery] int top = 5)
    {
        var result = await _aiService.GoiYPhoBienAsync(top);
        return Ok(result);
    }

    [HttpGet("thong-ke")]
    public async Task<IActionResult> ThongKe()
    {
        var tongKhoaHoc = await _db.KhoaHocs.CountAsync();
        var tongHocVien = await _db.HocViens.CountAsync();
        var tongGiangVien = await _db.GiangViens.CountAsync();
        var tongDangKy = await _db.DangKys.CountAsync();
        var doanhThu = await _db.DangKys.Where(d => d.NgayThanhToan != null).SumAsync(d => d.SoTienDaThanhToan);
        var khoaHocDangHoc = await _db.KhoaHocs.CountAsync(k => k.TrangThai == Models.TrangThaiKhoaHoc.DangHoc);
        var dangKyChoDuyet = await _db.DangKys.CountAsync(d => d.TrangThai == Models.TrangThaiDangKy.ChoDuyet);

        var khoaHocPhoBien = await _db.DangKys
            .Include(d => d.KhoaHoc)
            .GroupBy(d => d.KhoaHoc.TenKhoaHoc)
            .Select(g => new { TenKhoaHoc = g.Key, SoLuong = g.Count() })
            .OrderByDescending(x => x.SoLuong)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            TongKhoaHoc = tongKhoaHoc,
            TongHocVien = tongHocVien,
            TongGiangVien = tongGiangVien,
            TongDangKy = tongDangKy,
            DoanhThu = doanhThu,
            KhoaHocDangHoc = khoaHocDangHoc,
            DangKyChoDuyet = dangKyChoDuyet,
            KhoaHocPhoBien = khoaHocPhoBien
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("bao-cao")]
    public async Task<IActionResult> BaoCao()
    {
        var theoNgonNgu = await _db.KhoaHocs
            .GroupBy(k => k.NgonNgu)
            .Select(g => new { NgonNgu = g.Key, SoKhoaHoc = g.Count() })
            .ToListAsync();

        var theoTrinhDo = await _db.KhoaHocs
            .GroupBy(k => k.TrinhDo)
            .Select(g => new { TrinhDo = g.Key, SoKhoaHoc = g.Count() })
            .ToListAsync();

        // Doanh thu theo tháng (12 tháng gần nhất)
        var cutoff = DateTime.Now.AddMonths(-12);
        var rawDT = await _db.DangKys
            .Where(d => d.NgayThanhToan.HasValue && d.NgayThanhToan >= cutoff)
            .GroupBy(d => new { d.NgayThanhToan!.Value.Year, d.NgayThanhToan!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, DoanhThu = g.Sum(d => d.SoTienDaThanhToan), SoDangKy = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        // Nếu không có dữ liệu có ngày thanh toán, lấy tổng đăng ký theo tháng
        if (!rawDT.Any())
        {
            rawDT = await _db.DangKys
                .GroupBy(d => new { d.NgayDangKy.Year, d.NgayDangKy.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, DoanhThu = g.Sum(d => d.SoTienDaThanhToan), SoDangKy = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
        }
        var doanhThuTheoThang = rawDT
            .Select(x => new { Thang = $"{x.Month:D2}/{x.Year}", x.DoanhThu, x.SoDangKy })
            .ToList();

        var rawHV = await _db.HocViens
            .Where(h => h.NgayDangKy >= cutoff)
            .GroupBy(h => new { h.NgayDangKy.Year, h.NgayDangKy.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, SoLuong = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        // Nếu không có học viên mới trong 12 tháng, lấy tất cả
        if (!rawHV.Any())
        {
            rawHV = await _db.HocViens
                .GroupBy(h => new { h.NgayDangKy.Year, h.NgayDangKy.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, SoLuong = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
        }
        var hocVienMoiTheoThang = rawHV
            .Select(x => new { Thang = $"{x.Month:D2}/{x.Year}", x.SoLuong })
            .ToList();

        var diemPhanPhoi = await _db.Diems
            .Where(d => d.DiemTrungBinh.HasValue)
            .GroupBy(d => d.XepLoai)
            .Select(g => new { XepLoai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            TheoNgonNgu = theoNgonNgu,
            TheoTrinhDo = theoTrinhDo,
            DoanhThuTheoThang = doanhThuTheoThang,
            HocVienMoiTheoThang = hocVienMoiTheoThang,
            DiemPhanPhoi = diemPhanPhoi
        });
    }
}
