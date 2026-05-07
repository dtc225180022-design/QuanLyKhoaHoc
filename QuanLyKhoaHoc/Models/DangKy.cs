using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoaHoc.Models;

public class DangKy
{
    public int Id { get; set; }

    public int HocVienId { get; set; }
    public HocVien HocVien { get; set; } = null!;

    public int KhoaHocId { get; set; }
    public KhoaHoc KhoaHoc { get; set; } = null!;

    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;

    [MaxLength(50)]
    public string? PhuongThucThanhToan { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTienDaThanhToan { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    [MaxLength(200)]
    public string GhiChu { get; set; } = string.Empty;

    public Diem? Diem { get; set; }
}

public enum TrangThaiDangKy
{
    ChoDuyet = 1,
    DaDuyet = 2,
    DangHoc = 3,
    HoanThanh = 4,
    HuyBo = 5
}
