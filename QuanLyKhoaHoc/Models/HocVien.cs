using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class HocVien
{
    public int Id { get; set; }

    public int NguoiDungId { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;

    [MaxLength(20)]
    public string MaHocVien { get; set; } = string.Empty;

    public DateTime? NgaySinh { get; set; }

    [MaxLength(10)]
    public string GioiTinh { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DiaChi { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TrinhDoHienTai { get; set; } = string.Empty;

    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    public ICollection<DangKy> DanhSachDangKy { get; set; } = new List<DangKy>();
}
