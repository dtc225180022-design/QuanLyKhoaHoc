using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.DTOs;

public class DiemDto
{
    public int Id { get; set; }
    public int DangKyId { get; set; }
    public string TenHocVien { get; set; } = string.Empty;
    public string MaHocVien { get; set; } = string.Empty;
    public string TenKhoaHoc { get; set; } = string.Empty;
    public decimal? DiemChuyenCan { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTrungBinh { get; set; }
    public string XepLoai { get; set; } = string.Empty;
    public bool DaHoanThanh { get; set; }
    public string NhanXet { get; set; } = string.Empty;
    public DateTime? NgayCapNhat { get; set; }
}

public class CapNhatDiemDto
{
    [Range(0, 10)]
    public decimal? DiemGiuaKy { get; set; }

    [Range(0, 10)]
    public decimal? DiemCuoiKy { get; set; }

    [MaxLength(500)]
    public string NhanXet { get; set; } = string.Empty;

    public bool DaHoanThanh { get; set; }
}
