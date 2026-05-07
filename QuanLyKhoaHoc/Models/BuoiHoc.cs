using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class BuoiHoc
{
    public int Id { get; set; }

    public int KhoaHocId { get; set; }
    public KhoaHoc KhoaHoc { get; set; } = null!;

    public int? LichHocId { get; set; }
    public LichHoc? LichHoc { get; set; }

    public DateTime NgayHoc { get; set; }

    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }

    [MaxLength(200)]
    public string PhongHoc { get; set; } = string.Empty;

    [MaxLength(500)]
    public string LinkMeet { get; set; } = string.Empty;

    public HinhThucHoc HinhThuc { get; set; } = HinhThucHoc.Offline;

    public int SoBuoiThuTu { get; set; }

    [MaxLength(500)]
    public string GhiChu { get; set; } = string.Empty;

    public bool DaDienRa { get; set; }

    public ICollection<DiemDanh> DanhSachDiemDanh { get; set; } = new List<DiemDanh>();
}

public enum HinhThucHoc
{
    Offline = 1,
    Online = 2,
    KetHop = 3
}
