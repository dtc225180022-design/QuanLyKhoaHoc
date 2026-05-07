using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class Banner
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string TieuDe { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MoTa { get; set; } = string.Empty;

    /// <summary>URL ảnh (Unsplash / đường dẫn tương đối)</summary>
    [MaxLength(500)]
    public string HinhAnh { get; set; } = string.Empty;

    [MaxLength(300)]
    public string DuongDanLienKet { get; set; } = string.Empty;

    public int ThuTu { get; set; } = 1;

    public bool DangHienThi { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.Now;
}
