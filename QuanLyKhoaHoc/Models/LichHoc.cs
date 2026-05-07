using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc.Models;

public class LichHoc
{
    public int Id { get; set; }

    public int KhoaHocId { get; set; }
    public KhoaHoc KhoaHoc { get; set; } = null!;

    public DayOfWeek ThuTrongTuan { get; set; }

    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }

    [MaxLength(200)]
    public string PhongHoc { get; set; } = string.Empty;

    public HinhThucHoc HinhThuc { get; set; } = HinhThucHoc.Offline;

    [MaxLength(500)]
    public string LinkMeetMacDinh { get; set; } = string.Empty;

    [MaxLength(200)]
    public string GhiChu { get; set; } = string.Empty;

    public ICollection<BuoiHoc> DanhSachBuoiHoc { get; set; } = new List<BuoiHoc>();
}
