using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.DTOs;

public class GiangVienDto
{
    public int Id { get; set; }
    public int NguoiDungId { get; set; }
    public string MaGiangVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string AnhDaiDien { get; set; } = string.Empty;
    public string ChuyenNganh { get; set; } = string.Empty;
    public string BangCap { get; set; } = string.Empty;
    public int NamKinhNghiem { get; set; }
    public string GioiThieu { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; }
    public int SoKhoaHocPhuTrach { get; set; }
}

public class TaoGiangVienDto
{
    [Required]
    [MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string MatKhau { get; set; } = string.Empty;

    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string MaGiangVien { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ChuyenNganh { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BangCap { get; set; } = string.Empty;

    [Range(0, 50)]
    public int NamKinhNghiem { get; set; }

    [MaxLength(500)]
    public string GioiThieu { get; set; } = string.Empty;
}

public class CapNhatGiangVienDto
{
    [MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;
    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;
    [MaxLength(100)]
    public string ChuyenNganh { get; set; } = string.Empty;
    [MaxLength(100)]
    public string BangCap { get; set; } = string.Empty;
    [Range(0, 50)]
    public int NamKinhNghiem { get; set; }
    [MaxLength(500)]
    public string GioiThieu { get; set; } = string.Empty;
}

public class PhanCongDto
{
    public int Id { get; set; }
    public string TenGiangVien { get; set; } = string.Empty;
    public string MaGiangVien { get; set; } = string.Empty;
    public string TenKhoaHoc { get; set; } = string.Empty;
    public string MaKhoaHoc { get; set; } = string.Empty;
    public DateTime NgayPhanCong { get; set; }
    public bool DangHoatDong { get; set; }
}

public class TaoPhanCongDto
{
    [Required]
    public int GiangVienId { get; set; }
    [Required]
    public int KhoaHocId { get; set; }
    [MaxLength(200)]
    public string GhiChu { get; set; } = string.Empty;
}
