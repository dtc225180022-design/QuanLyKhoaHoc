using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string MatKhau { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [MaxLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string MatKhau { get; set; } = string.Empty;

    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public string AnhDaiDien { get; set; } = string.Empty;
    public DateTime HetHan { get; set; }
}

public class DoiMatKhauDto
{
    [Required]
    public string MatKhauCu { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string MatKhauMoi { get; set; } = string.Empty;
}
