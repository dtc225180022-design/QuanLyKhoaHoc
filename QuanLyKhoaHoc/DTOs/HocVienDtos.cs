using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.DTOs;

public class HocVienDto
{
    public int Id { get; set; }
    public int NguoiDungId { get; set; }
    public string MaHocVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string AnhDaiDien { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public string GioiTinh { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string TrinhDoHienTai { get; set; } = string.Empty;
    public DateTime NgayDangKy { get; set; }
    public int SoKhoaHocDangHoc { get; set; }
}

public class CapNhatHocVienDto
{
    [MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    [MaxLength(10)]
    public string GioiTinh { get; set; } = string.Empty;
    [MaxLength(200)]
    public string DiaChi { get; set; } = string.Empty;
    [MaxLength(100)]
    public string TrinhDoHienTai { get; set; } = string.Empty;
    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;
}

public class DangKyKhoaHocDto
{
    [Required]
    public int KhoaHocId { get; set; }
    [MaxLength(200)]
    public string GhiChu { get; set; } = string.Empty;
}

public class DangKyDto
{
    public int Id { get; set; }
    public string TenHocVien { get; set; } = string.Empty;
    public string MaHocVien { get; set; } = string.Empty;
    public string TenKhoaHoc { get; set; } = string.Empty;
    public string MaKhoaHoc { get; set; } = string.Empty;
    public DateTime NgayDangKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public decimal SoTienDaThanhToan { get; set; }
    public decimal HocPhi { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string? PhuongThucThanhToan { get; set; }
    public string GhiChu { get; set; } = string.Empty;
}

public class CapNhatTrangThaiDangKyDto
{
    [Required]
    public int TrangThai { get; set; }
    [MaxLength(200)]
    public string GhiChu { get; set; } = string.Empty;
}

public class ChonPhuongThucDto
{
    [Required]
    [MaxLength(50)]
    public string PhuongThuc { get; set; } = string.Empty;
}

public class XacNhanThanhToanDto
{
    public decimal SoTien { get; set; }
    [MaxLength(50)]
    public string? PhuongThuc { get; set; }
}
