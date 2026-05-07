using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoaHoc.Models;

public class KhoaHoc
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string MaKhoaHoc { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string TenKhoaHoc { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string MoTa { get; set; } = string.Empty;

    [MaxLength(50)]
    public string NgonNgu { get; set; } = string.Empty;

    [MaxLength(50)]
    public string TrinhDo { get; set; } = string.Empty;

    public int SoBuoiHoc { get; set; }

    public int ThoiLuongMoiBuoi { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HocPhi { get; set; }

    public int SoLuongToiDa { get; set; }

    public TrangThaiKhoaHoc TrangThai { get; set; } = TrangThaiKhoaHoc.MoChuaHoc;

    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<LichHoc> DanhSachLichHoc { get; set; } = new List<LichHoc>();
    public ICollection<DangKy> DanhSachDangKy { get; set; } = new List<DangKy>();
    public ICollection<PhanCong> DanhSachPhanCong { get; set; } = new List<PhanCong>();
    public ICollection<BuoiHoc> DanhSachBuoiHoc { get; set; } = new List<BuoiHoc>();
}

public enum TrangThaiKhoaHoc
{
    MoChuaHoc = 1,
    DangHoc = 2,
    DaKetThuc = 3,
    HuyBo = 4
}
