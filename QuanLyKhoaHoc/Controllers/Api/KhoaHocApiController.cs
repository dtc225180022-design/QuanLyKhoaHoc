using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Controllers.Api;

[ApiController]
[Route("api/khoa-hoc")]
public class KhoaHocApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public KhoaHocApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? ngonNgu,
        [FromQuery] string? trinhDo,
        [FromQuery] string? trangThai,
        [FromQuery] string? search)
    {
        var query = _db.KhoaHocs
            .Include(k => k.DanhSachLichHoc)
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .AsQueryable();

        if (!string.IsNullOrEmpty(ngonNgu))
            query = query.Where(k => k.NgonNgu == ngonNgu);
        if (!string.IsNullOrEmpty(trinhDo))
            query = query.Where(k => k.TrinhDo == trinhDo);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(k => k.TenKhoaHoc.Contains(search) || k.MaKhoaHoc.Contains(search));
        if (!string.IsNullOrEmpty(trangThai) && Enum.TryParse<TrangThaiKhoaHoc>(trangThai, out var ts))
            query = query.Where(k => k.TrangThai == ts);

        var khoaHocs = await query.OrderByDescending(k => k.NgayTao).ToListAsync();
        return Ok(khoaHocs.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var kh = await _db.KhoaHocs
            .Include(k => k.DanhSachLichHoc)
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kh == null) return NotFound();
        return Ok(MapToDto(kh));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaoKhoaHocDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _db.KhoaHocs.AnyAsync(k => k.MaKhoaHoc == dto.MaKhoaHoc))
            return BadRequest(new { message = "Mã khóa học đã tồn tại" });

        var khoaHoc = new KhoaHoc
        {
            MaKhoaHoc = dto.MaKhoaHoc,
            TenKhoaHoc = dto.TenKhoaHoc,
            MoTa = dto.MoTa,
            NgonNgu = dto.NgonNgu,
            TrinhDo = dto.TrinhDo,
            SoBuoiHoc = dto.SoBuoiHoc,
            ThoiLuongMoiBuoi = dto.ThoiLuongMoiBuoi,
            HocPhi = dto.HocPhi,
            SoLuongToiDa = dto.SoLuongToiDa,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc,
            DanhSachLichHoc = dto.DanhSachLichHoc.Select(l => new LichHoc
            {
                ThuTrongTuan = (DayOfWeek)l.ThuTrongTuan,
                GioBatDau = l.GioBatDau,
                GioKetThuc = l.GioKetThuc,
                PhongHoc = l.PhongHoc
            }).ToList()
        };

        _db.KhoaHocs.Add(khoaHoc);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = khoaHoc.Id }, MapToDto(khoaHoc));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaoKhoaHocDto dto)
    {
        var kh = await _db.KhoaHocs.Include(k => k.DanhSachLichHoc).FirstOrDefaultAsync(k => k.Id == id);
        if (kh == null) return NotFound();

        kh.TenKhoaHoc = dto.TenKhoaHoc;
        kh.MoTa = dto.MoTa;
        kh.NgonNgu = dto.NgonNgu;
        kh.TrinhDo = dto.TrinhDo;
        kh.SoBuoiHoc = dto.SoBuoiHoc;
        kh.ThoiLuongMoiBuoi = dto.ThoiLuongMoiBuoi;
        kh.HocPhi = dto.HocPhi;
        kh.SoLuongToiDa = dto.SoLuongToiDa;
        kh.NgayBatDau = dto.NgayBatDau;
        kh.NgayKetThuc = dto.NgayKetThuc;

        _db.LichHocs.RemoveRange(kh.DanhSachLichHoc);
        kh.DanhSachLichHoc = dto.DanhSachLichHoc.Select(l => new LichHoc
        {
            KhoaHocId = id,
            ThuTrongTuan = (DayOfWeek)l.ThuTrongTuan,
            GioBatDau = l.GioBatDau,
            GioKetThuc = l.GioKetThuc,
            PhongHoc = l.PhongHoc
        }).ToList();

        await _db.SaveChangesAsync();
        return Ok(MapToDto(kh));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/trang-thai")]
    public async Task<IActionResult> CapNhatTrangThai(int id, [FromBody] int trangThai)
    {
        var kh = await _db.KhoaHocs.FindAsync(id);
        if (kh == null) return NotFound();
        kh.TrangThai = (TrangThaiKhoaHoc)trangThai;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật trạng thái thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var kh = await _db.KhoaHocs.Include(k => k.DanhSachDangKy).FirstOrDefaultAsync(k => k.Id == id);
        if (kh == null) return NotFound();

        if (kh.DanhSachDangKy.Any(d => d.TrangThai == TrangThaiDangKy.DangHoc))
            return BadRequest(new { message = "Không thể xóa khóa học đang có học viên" });

        _db.KhoaHocs.Remove(kh);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Xóa khóa học thành công" });
    }

    [Authorize(Roles = "Admin,GiangVien")]
    [HttpGet("{id}/hoc-vien")]
    public async Task<IActionResult> GetHocVienTrongKhoaHoc(int id)
    {
        var kh = await _db.KhoaHocs
            .Include(k => k.DanhSachDangKy)
                .ThenInclude(dk => dk.HocVien)
                    .ThenInclude(hv => hv.NguoiDung)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kh == null) return NotFound(new { message = "Không tìm thấy khóa học" });

        var result = kh.DanhSachDangKy
            .Where(dk => dk.TrangThai != TrangThaiDangKy.HuyBo)
            .OrderBy(dk => dk.NgayDangKy)
            .Select(dk => new
            {
                id = dk.HocVien.Id,
                hoTen = dk.HocVien.NguoiDung.HoTen,
                email = dk.HocVien.NguoiDung.Email,
                soDienThoai = dk.HocVien.NguoiDung.SoDienThoai,
                anhDaiDien = dk.HocVien.NguoiDung.AnhDaiDien,
                maHocVien = dk.HocVien.MaHocVien,
                trangThaiDangKy = dk.TrangThai.ToString(),
                ngayDangKy = dk.NgayDangKy
            });

        return Ok(result);
    }

    private static KhoaHocDto MapToDto(KhoaHoc k) => new()
    {
        Id = k.Id,
        MaKhoaHoc = k.MaKhoaHoc,
        TenKhoaHoc = k.TenKhoaHoc,
        MoTa = k.MoTa,
        NgonNgu = k.NgonNgu,
        TrinhDo = k.TrinhDo,
        SoBuoiHoc = k.SoBuoiHoc,
        ThoiLuongMoiBuoi = k.ThoiLuongMoiBuoi,
        HocPhi = k.HocPhi,
        SoLuongToiDa = k.SoLuongToiDa,
        SoHocVienHienTai = k.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo),
        TrangThai = k.TrangThai.ToString(),
        NgayBatDau = k.NgayBatDau,
        NgayKetThuc = k.NgayKetThuc,
        TenGiangVien = k.DanhSachPhanCong.FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen,
        DanhSachLichHoc = k.DanhSachLichHoc.Select(l => new LichHocDto
        {
            Id = l.Id,
            ThuTrongTuan = l.ThuTrongTuan.ToString(),
            GioBatDau = l.GioBatDau.ToString(@"hh\:mm"),
            GioKetThuc = l.GioKetThuc.ToString(@"hh\:mm"),
            PhongHoc = l.PhongHoc
        }).ToList()
    };
}
