using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoaHoc.Models;

public class Diem
{
    public int Id { get; set; }

    public int DangKyId { get; set; }
    public DangKy DangKy { get; set; } = null!;

    // Chuyên cần: 10% - tính từ DiemDanh
    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemChuyenCan { get; set; }

    // Giữa kỳ: 30%
    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemGiuaKy { get; set; }

    // Cuối kỳ: 60%
    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemCuoiKy { get; set; }

    // Tổng kết = ChuyenCan*10% + GiuaKy*30% + CuoiKy*60%
    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemTongKet { get; set; }

    // Giữ lại để tương thích (= DiemTongKet)
    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiemTrungBinh { get; set; }

    [MaxLength(30)]
    public string XepLoai { get; set; } = string.Empty;

    public bool DaHoanThanh { get; set; }
    public bool DuDieuKienCapChungChi { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [MaxLength(500)]
    public string NhanXet { get; set; } = string.Empty;
}
