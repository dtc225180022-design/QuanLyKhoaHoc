using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Services;

public class GoiYKhoaHocDto
{
    public int KhoaHocId { get; set; }
    public string TenKhoaHoc { get; set; } = string.Empty;
    public string MaKhoaHoc { get; set; } = string.Empty;
    public string NgonNgu { get; set; } = string.Empty;
    public string TrinhDo { get; set; } = string.Empty;
    public decimal HocPhi { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public double DiemPhuHop { get; set; }
    public string NgayBatDau { get; set; } = string.Empty;
    public int SoChoConLai { get; set; }
    public string TenGiangVien { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public interface IAIRecommendService
{
    Task<List<GoiYKhoaHocDto>> GoiYKhoaHocAsync(int hocVienId);
    Task<List<GoiYKhoaHocDto>> GoiYTheoTrinhDoAsync(string trinhDo, string ngonNgu);
    Task<List<GoiYKhoaHocDto>> GoiYPhoBienAsync(int top = 5);
}

public class AIRecommendService : IAIRecommendService
{
    private readonly ApplicationDbContext _db;
    private static readonly Dictionary<string, string> _capTiepTheo = new()
    {
        ["Căn bản"] = "Trung cấp",
        ["Trung cấp"] = "Nâng cao",
        ["Nâng cao"] = "Chuyên sâu"
    };

    public AIRecommendService(ApplicationDbContext db) => _db = db;

    public async Task<List<GoiYKhoaHocDto>> GoiYKhoaHocAsync(int hocVienId)
    {
        var hocVien = await _db.HocViens
            .Include(h => h.DanhSachDangKy).ThenInclude(d => d.KhoaHoc)
            .Include(h => h.DanhSachDangKy).ThenInclude(d => d.Diem)
            .FirstOrDefaultAsync(h => h.Id == hocVienId);

        if (hocVien == null) return new();

        var daDangKyIds = hocVien.DanhSachDangKy.Select(d => d.KhoaHocId).ToHashSet();

        var tatCaKhoaHoc = await _db.KhoaHocs
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Where(k => k.TrangThai == TrangThaiKhoaHoc.MoChuaHoc && !daDangKyIds.Contains(k.Id))
            .ToListAsync();

        var scoredItems = tatCaKhoaHoc
            .Select(k => (khoaHoc: k, score: TinhDiemPhuHop(hocVien, k, tatCaKhoaHoc)))
            .Where(x => x.score.diem > 0)
            .OrderByDescending(x => x.score.diem)
            .Take(6)
            .ToList();

        return scoredItems.Select(x => new GoiYKhoaHocDto
        {
            KhoaHocId = x.khoaHoc.Id,
            TenKhoaHoc = x.khoaHoc.TenKhoaHoc,
            MaKhoaHoc = x.khoaHoc.MaKhoaHoc,
            NgonNgu = x.khoaHoc.NgonNgu,
            TrinhDo = x.khoaHoc.TrinhDo,
            HocPhi = x.khoaHoc.HocPhi,
            LyDo = x.score.lyDo,
            DiemPhuHop = x.score.diem,
            NgayBatDau = x.khoaHoc.NgayBatDau.ToString("dd/MM/yyyy"),
            SoChoConLai = Math.Max(0, x.khoaHoc.SoLuongToiDa - x.khoaHoc.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo)),
            TenGiangVien = x.khoaHoc.DanhSachPhanCong.FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen ?? "Chưa phân công",
            Tags = TaoTags(hocVien, x.khoaHoc)
        }).ToList();
    }

    public async Task<List<GoiYKhoaHocDto>> GoiYTheoTrinhDoAsync(string trinhDo, string ngonNgu)
    {
        var query = _db.KhoaHocs
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Where(k => k.TrangThai == TrangThaiKhoaHoc.MoChuaHoc);

        if (!string.IsNullOrEmpty(ngonNgu))
            query = query.Where(k => k.NgonNgu.Contains(ngonNgu));
        if (!string.IsNullOrEmpty(trinhDo))
            query = query.Where(k => k.TrinhDo == trinhDo);

        var list = await query.ToListAsync();
        return list.Select(k => new GoiYKhoaHocDto
        {
            KhoaHocId = k.Id,
            TenKhoaHoc = k.TenKhoaHoc,
            MaKhoaHoc = k.MaKhoaHoc,
            NgonNgu = k.NgonNgu,
            TrinhDo = k.TrinhDo,
            HocPhi = k.HocPhi,
            LyDo = $"Phù hợp với trình độ {trinhDo} - {ngonNgu}",
            DiemPhuHop = 85,
            NgayBatDau = k.NgayBatDau.ToString("dd/MM/yyyy"),
            SoChoConLai = Math.Max(0, k.SoLuongToiDa - k.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo)),
            TenGiangVien = k.DanhSachPhanCong.FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen ?? "Chưa phân công"
        }).ToList();
    }

    public async Task<List<GoiYKhoaHocDto>> GoiYPhoBienAsync(int top = 5)
    {
        var khoaHocs = await _db.KhoaHocs
            .Include(k => k.DanhSachDangKy)
            .Include(k => k.DanhSachPhanCong).ThenInclude(p => p.GiangVien).ThenInclude(g => g.NguoiDung)
            .Where(k => k.TrangThai == TrangThaiKhoaHoc.MoChuaHoc)
            .ToListAsync();

        return khoaHocs
            .OrderByDescending(k => k.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo))
            .Take(top)
            .Select(k => new GoiYKhoaHocDto
            {
                KhoaHocId = k.Id,
                TenKhoaHoc = k.TenKhoaHoc,
                MaKhoaHoc = k.MaKhoaHoc,
                NgonNgu = k.NgonNgu,
                TrinhDo = k.TrinhDo,
                HocPhi = k.HocPhi,
                LyDo = "Khóa học phổ biến nhất",
                DiemPhuHop = 70,
                NgayBatDau = k.NgayBatDau.ToString("dd/MM/yyyy"),
                SoChoConLai = Math.Max(0, k.SoLuongToiDa - k.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo)),
                TenGiangVien = k.DanhSachPhanCong.FirstOrDefault(p => p.DangHoatDong)?.GiangVien?.NguoiDung?.HoTen ?? "Chưa phân công"
            }).ToList();
    }

    private (double diem, string lyDo) TinhDiemPhuHop(HocVien hocVien, KhoaHoc khoaHoc, List<KhoaHoc> tatCa)
    {
        double diem = 40.0;
        var lyDos = new List<string>();

        var ngonNguDaHoc = hocVien.DanhSachDangKy
            .GroupBy(d => d.KhoaHoc?.NgonNgu ?? "")
            .ToDictionary(g => g.Key, g => g.ToList());

        // 1. Cùng ngôn ngữ đang học → ưu tiên cao
        if (ngonNguDaHoc.ContainsKey(khoaHoc.NgonNgu))
        {
            diem += 25;
            lyDos.Add($"Tiếp tục lộ trình {khoaHoc.NgonNgu}");

            // Kiểm tra cấp tiếp theo
            var capDaHoc = ngonNguDaHoc[khoaHoc.NgonNgu]
                .Select(d => d.KhoaHoc?.TrinhDo ?? "")
                .ToHashSet();

            foreach (var cap in capDaHoc)
            {
                if (_capTiepTheo.TryGetValue(cap, out var capTiep) && capTiep == khoaHoc.TrinhDo)
                {
                    diem += 20;
                    lyDos.Add($"Cấp độ tiếp theo sau {cap}");
                    break;
                }
            }

            // Nếu điểm tốt → gợi ý nâng cấp mạnh hơn
            var diemTot = hocVien.DanhSachDangKy
                .Where(d => d.KhoaHoc?.NgonNgu == khoaHoc.NgonNgu && d.Diem?.DiemTrungBinh >= 8)
                .Any();
            if (diemTot)
            {
                diem += 10;
                lyDos.Add("Dựa trên kết quả học tập tốt");
            }
        }
        else
        {
            // Ngôn ngữ mới
            diem += 5;
            lyDos.Add("Mở rộng ngoại ngữ mới");
        }

        // 2. Trình độ khớp với hồ sơ học viên
        var trinhDoHv = hocVien.TrinhDoHienTai?.ToLower() ?? "";
        if ((trinhDoHv.Contains("sơ") || trinhDoHv.Contains("mới")) && khoaHoc.TrinhDo == "Căn bản")
        {
            diem += 15;
            lyDos.Add("Phù hợp trình độ hiện tại");
        }
        else if (trinhDoHv.Contains("trung") && khoaHoc.TrinhDo == "Trung cấp")
        {
            diem += 15;
        }

        // 3. Khóa học sắp khai giảng (ưu tiên trong 30 ngày)
        var ngayConLai = (khoaHoc.NgayBatDau - DateTime.Now).TotalDays;
        if (ngayConLai is >= 0 and <= 14)
        {
            diem += 12;
            lyDos.Add("Khai giảng sắp tới");
        }
        else if (ngayConLai is > 14 and <= 30)
        {
            diem += 6;
        }

        // 4. Còn nhiều chỗ
        var soHocVien = khoaHoc.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo);
        var tyLe = khoaHoc.SoLuongToiDa > 0 ? (double)soHocVien / khoaHoc.SoLuongToiDa : 1;
        if (tyLe < 0.5) { diem += 8; }
        else if (tyLe >= 0.9) { diem -= 15; } // Gần đầy → ít gợi ý

        // 5. Khóa phổ biến (nhiều học viên đăng ký)
        var maxHocVien = tatCa.Max(k => k.DanhSachDangKy.Count);
        if (maxHocVien > 0)
            diem += (double)soHocVien / maxHocVien * 8;

        // 6. Chưa từng học ngôn ngữ này (khám phá mới)
        if (!ngonNguDaHoc.Any()) { lyDos.Add("Khám phá ngôn ngữ đầu tiên"); diem += 10; }

        var lyDo = lyDos.Any() ? string.Join(" · ", lyDos) : "Khóa học phù hợp với bạn";
        return (Math.Min(100, Math.Max(0, diem)), lyDo);
    }

    private static List<string> TaoTags(HocVien hocVien, KhoaHoc khoaHoc)
    {
        var tags = new List<string>();
        var ngayConLai = (khoaHoc.NgayBatDau - DateTime.Now).TotalDays;

        if (ngayConLai is >= 0 and <= 7) tags.Add("🔥 Sắp khai giảng");

        var ngonNguDaHoc = hocVien.DanhSachDangKy.Select(d => d.KhoaHoc?.NgonNgu ?? "").ToHashSet();
        if (ngonNguDaHoc.Contains(khoaHoc.NgonNgu)) tags.Add("📈 Lộ trình của bạn");

        var soHocVien = khoaHoc.DanhSachDangKy.Count(d => d.TrangThai != TrangThaiDangKy.HuyBo);
        if (soHocVien >= khoaHoc.SoLuongToiDa * 0.7) tags.Add("⚡ Gần đầy");

        var diemTot = hocVien.DanhSachDangKy
            .Where(d => d.KhoaHoc?.NgonNgu == khoaHoc.NgonNgu)
            .Any(d => d.Diem?.DiemTrungBinh >= 8);
        if (diemTot) tags.Add("⭐ Dựa trên điểm số");

        return tags;
    }
}
