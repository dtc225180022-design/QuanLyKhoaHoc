using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class NguoiDung
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MatKhauHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AnhDaiDien { get; set; } = string.Empty;

    public VaiTro VaiTro { get; set; } = VaiTro.User;

    public bool HoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public HocVien? HocVien { get; set; }
    public GiangVien? GiangVien { get; set; }
}

public enum VaiTro
{
    Admin = 1,
    GiangVien = 2,
    User = 3
}
