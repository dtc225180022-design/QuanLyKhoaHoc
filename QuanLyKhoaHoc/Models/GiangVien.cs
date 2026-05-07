using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class GiangVien
{
    public int Id { get; set; }

    public int NguoiDungId { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;

    [MaxLength(20)]
    public string MaGiangVien { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ChuyenNganh { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BangCap { get; set; } = string.Empty;

    public int NamKinhNghiem { get; set; }

    [MaxLength(500)]
    public string GioiThieu { get; set; } = string.Empty;

    public bool DangHoatDong { get; set; } = true;

    public ICollection<PhanCong> DanhSachPhanCong { get; set; } = new List<PhanCong>();
}
