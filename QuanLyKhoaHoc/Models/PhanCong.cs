using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class PhanCong
{
    public int Id { get; set; }

    public int GiangVienId { get; set; }
    public GiangVien GiangVien { get; set; } = null!;

    public int KhoaHocId { get; set; }
    public KhoaHoc KhoaHoc { get; set; } = null!;

    public DateTime NgayPhanCong { get; set; } = DateTime.Now;

    [MaxLength(200)]
    public string GhiChu { get; set; } = string.Empty;

    public bool DangHoatDong { get; set; } = true;
}
