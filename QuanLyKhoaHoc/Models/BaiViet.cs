using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class BaiViet
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string TieuDe { get; set; } = string.Empty;

    [MaxLength(500)]
    public string TomTat { get; set; } = string.Empty;

    public string NoiDung { get; set; } = string.Empty;

    [MaxLength(500)]
    public string HinhAnh { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TheLoai { get; set; } = "Tin tức";

    [MaxLength(300)]
    public string TacGia { get; set; } = string.Empty;

    public bool DangHienThi { get; set; } = true;

    public bool NoiBat { get; set; } = false;

    public int LuotXem { get; set; } = 0;

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiTaoId { get; set; }
    public NguoiDung? NguoiTao { get; set; }
}
