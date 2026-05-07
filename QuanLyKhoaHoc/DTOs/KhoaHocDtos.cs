using System.ComponentModel.DataAnnotations;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.DTOs;

public class KhoaHocDto
{
    public int Id { get; set; }
    public string MaKhoaHoc { get; set; } = string.Empty;
    public string TenKhoaHoc { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string NgonNgu { get; set; } = string.Empty;
    public string TrinhDo { get; set; } = string.Empty;
    public int SoBuoiHoc { get; set; }
    public int ThoiLuongMoiBuoi { get; set; }
    public decimal HocPhi { get; set; }
    public int SoLuongToiDa { get; set; }
    public int SoHocVienHienTai { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public List<LichHocDto> DanhSachLichHoc { get; set; } = new();
    public string? TenGiangVien { get; set; }
}

public class TaoKhoaHocDto
{
    [Required(ErrorMessage = "Mã khóa học không được để trống")]
    [MaxLength(20)]
    public string MaKhoaHoc { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khóa học không được để trống")]
    [MaxLength(200)]
    public string TenKhoaHoc { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string MoTa { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string NgonNgu { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TrinhDo { get; set; } = string.Empty;

    [Range(1, 365)]
    public int SoBuoiHoc { get; set; }

    [Range(30, 480)]
    public int ThoiLuongMoiBuoi { get; set; }

    [Range(0, 100000000)]
    public decimal HocPhi { get; set; }

    [Range(1, 100)]
    public int SoLuongToiDa { get; set; }

    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }

    public List<TaoLichHocDto> DanhSachLichHoc { get; set; } = new();
}

public class LichHocDto
{
    public int Id { get; set; }
    public string ThuTrongTuan { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public string PhongHoc { get; set; } = string.Empty;
}

public class TaoLichHocDto
{
    public int ThuTrongTuan { get; set; }
    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }
    [MaxLength(100)]
    public string PhongHoc { get; set; } = string.Empty;
}
