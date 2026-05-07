using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class DiemDanh
{
    public int Id { get; set; }

    public int BuoiHocId { get; set; }
    public BuoiHoc BuoiHoc { get; set; } = null!;

    public int DangKyId { get; set; }
    public DangKy DangKy { get; set; } = null!;

    public TrangThaiDiemDanh TrangThai { get; set; } = TrangThaiDiemDanh.CoMat;

    [MaxLength(300)]
    public string GhiChu { get; set; } = string.Empty;

    public DateTime NgayDiemDanh { get; set; } = DateTime.Now;
}

public enum TrangThaiDiemDanh
{
    CoMat = 1,
    Vang = 2,
    VangCoPhep = 3,
    HocOnline = 4
}
