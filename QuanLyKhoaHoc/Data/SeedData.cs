using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (context.NguoiDungs.Any()) return;

        // ─── Mật khẩu ────────────────────────────────────────────────────
        var adminPass = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var gvPass    = BCrypt.Net.BCrypt.HashPassword("Gv@123");
        var hvPass    = BCrypt.Net.BCrypt.HashPassword("User@123");

        // ─── Admin ───────────────────────────────────────────────────────
        var admin = new NguoiDung { HoTen="Quản trị viên", Email="admin@educenter.vn", MatKhauHash=adminPass, SoDienThoai="19006868", VaiTro=VaiTro.Admin };
        context.NguoiDungs.Add(admin);
        context.SaveChanges();

        // ─── 5 Giảng viên ────────────────────────────────────────────────
        var gvUserData = new[]
        {
            ("Nguyễn Thị Hương",  "huong.gv@educenter.vn",  "0912345678"),
            ("Trần Văn Minh",     "minh.gv@educenter.vn",   "0923456789"),
            ("Park Ji Young",     "park.gv@educenter.vn",   "0934111222"),
            ("Lý Văn Đức",        "duc.gv@educenter.vn",    "0945222333"),
            ("Sophie Nguyen",     "sophie.gv@educenter.vn", "0956333444"),
        };
        var gvUsers = gvUserData.Select(g => new NguoiDung
        {
            HoTen=g.Item1, Email=g.Item2, MatKhauHash=gvPass, SoDienThoai=g.Item3, VaiTro=VaiTro.GiangVien
        }).ToArray();
        context.NguoiDungs.AddRange(gvUsers);
        context.SaveChanges();

        var giangViens = new[]
        {
            new GiangVien { NguoiDungId=gvUsers[0].Id, MaGiangVien="GV001", ChuyenNganh="Tiếng Anh",   BangCap="Thạc sĩ Ngôn ngữ Anh – ĐH Hà Nội",         NamKinhNghiem=8, GioiThieu="Chuyên gia tiếng Anh giao tiếp và luyện thi IELTS, 8 năm kinh nghiệm giảng dạy tại các trung tâm hàng đầu.",   DangHoatDong=true },
            new GiangVien { NguoiDungId=gvUsers[1].Id, MaGiangVien="GV002", ChuyenNganh="Tiếng Nhật",  BangCap="Cử nhân Ngôn ngữ Nhật – ĐH Ngoại ngữ",      NamKinhNghiem=5, GioiThieu="Chuyên gia tiếng Nhật, chứng chỉ JLPT N1, 3 năm sống và làm việc tại Nhật Bản.",                             DangHoatDong=true },
            new GiangVien { NguoiDungId=gvUsers[2].Id, MaGiangVien="GV003", ChuyenNganh="Tiếng Hàn",   BangCap="Thạc sĩ Hàn Quốc học – ĐH KHXH&NV",         NamKinhNghiem=6, GioiThieu="Giáo viên bản ngữ người Hàn Quốc, chứng chỉ TOPIK 6, chuyên gia giảng dạy K-Pop Culture.",                  DangHoatDong=true },
            new GiangVien { NguoiDungId=gvUsers[3].Id, MaGiangVien="GV004", ChuyenNganh="Tiếng Trung",  BangCap="Cử nhân Ngôn ngữ Trung – ĐH Ngoại thương",   NamKinhNghiem=7, GioiThieu="Chuyên gia tiếng Trung thương mại, chứng chỉ HSK 6, 4 năm làm việc tại Quảng Châu.",                        DangHoatDong=true },
            new GiangVien { NguoiDungId=gvUsers[4].Id, MaGiangVien="GV005", ChuyenNganh="Tiếng Pháp",  BangCap="Thạc sĩ Ngôn ngữ Pháp – ĐH Ngoại ngữ HN",   NamKinhNghiem=4, GioiThieu="Cô giáo người Pháp gốc Việt, chứng chỉ DALF C1, đam mê truyền cảm hứng học ngoại ngữ.",                  DangHoatDong=true },
        };
        context.GiangViens.AddRange(giangViens);
        context.SaveChanges();

        // ─── 20 Học viên ─────────────────────────────────────────────────
        var hvRawData = new (string Ten, string Email, string Phone, DateTime NgaySinh, string GioiTinh, string DiaChi, string TrinhDo, string Ma, DateTime NgayDK)[]
        {
            ("Lê Thị Mai",        "mai.hv@gmail.com",      "0934567890", new DateTime(2000,5,15),  "Nữ",  "45 Đinh Tiên Hoàng, Hoàn Kiếm, Hà Nội",        "Sơ cấp",    "HV001", new DateTime(2025,8,15)),
            ("Phạm Văn An",       "an.hv@gmail.com",       "0945678901", new DateTime(1998,8,20),  "Nam", "67 Bà Triệu, Hai Bà Trưng, Hà Nội",             "Trung cấp", "HV002", new DateTime(2025,9,1)),
            ("Trần Thị Bích",     "bich.hv@gmail.com",     "0956789012", new DateTime(2001,3,10),  "Nữ",  "12 Nguyễn Huệ, Cầu Giấy, Hà Nội",              "Sơ cấp",    "HV003", new DateTime(2025,9,5)),
            ("Nguyễn Văn Cường",  "cuong.hv@gmail.com",    "0967890123", new DateTime(1997,11,5),  "Nam", "88 Trần Phú, Hà Đông, Hà Nội",                  "Nâng cao",  "HV004", new DateTime(2025,9,10)),
            ("Hoàng Thị Dung",    "dung.hv@gmail.com",     "0978901234", new DateTime(2002,7,22),  "Nữ",  "34 Lê Lợi, Nam Từ Liêm, Hà Nội",               "Sơ cấp",    "HV005", new DateTime(2025,10,3)),
            ("Vũ Minh Đức",       "duc.hv@gmail.com",      "0989012345", new DateTime(1999,4,18),  "Nam", "56 Phố Huế, Hai Bà Trưng, Hà Nội",              "Trung cấp", "HV006", new DateTime(2025,10,20)),
            ("Đỗ Thị Hà",         "ha.hv@gmail.com",       "0990123456", new DateTime(2003,1,30),  "Nữ",  "23 Hoàng Diệu, Ba Đình, Hà Nội",                "Sơ cấp",    "HV007", new DateTime(2025,11,5)),
            ("Bùi Văn Hải",       "hai.hv@gmail.com",      "0901234567", new DateTime(1996,9,12),  "Nam", "91 Nguyễn Trãi, Thanh Xuân, Hà Nội",            "Nâng cao",  "HV008", new DateTime(2025,11,15)),
            ("Lý Thị Hoa",        "hoa.hv@gmail.com",      "0912345670", new DateTime(2001,6,8),   "Nữ",  "78 Kim Mã, Ba Đình, Hà Nội",                    "Trung cấp", "HV009", new DateTime(2025,12,1)),
            ("Đinh Văn Hùng",     "hung.hv@gmail.com",     "0923456781", new DateTime(1995,12,25), "Nam", "15 Cầu Giấy, Cầu Giấy, Hà Nội",                "Sơ cấp",    "HV010", new DateTime(2025,12,10)),
            ("Mai Thị Lan",       "lan.hv@gmail.com",      "0934567892", new DateTime(2000,2,14),  "Nữ",  "62 Xuân Thủy, Cầu Giấy, Hà Nội",               "Trung cấp", "HV011", new DateTime(2026,1,5)),
            ("Phan Văn Long",     "long.hv@gmail.com",     "0945678903", new DateTime(1998,10,3),  "Nam", "47 Trung Kính, Cầu Giấy, Hà Nội",               "Sơ cấp",    "HV012", new DateTime(2026,1,10)),
            ("Tô Thị Minh",       "minh2.hv@gmail.com",    "0956789014", new DateTime(2002,4,19),  "Nữ",  "33 Hoàng Quốc Việt, Cầu Giấy, Hà Nội",         "Sơ cấp",    "HV013", new DateTime(2026,1,20)),
            ("Lê Văn Nam",        "nam.hv@gmail.com",      "0967890125", new DateTime(1997,7,7),   "Nam", "19 Đường Láng, Đống Đa, Hà Nội",                "Nâng cao",  "HV014", new DateTime(2026,2,1)),
            ("Ngô Thị Nga",       "nga.hv@gmail.com",      "0978901236", new DateTime(2003,9,21),  "Nữ",  "5 Văn Cao, Ba Đình, Hà Nội",                    "Sơ cấp",    "HV015", new DateTime(2026,2,5)),
            ("Cao Minh Phúc",     "phuc.hv@gmail.com",     "0989012347", new DateTime(1999,3,15),  "Nam", "82 Liễu Giai, Ba Đình, Hà Nội",                 "Trung cấp", "HV016", new DateTime(2026,2,10)),
            ("Trịnh Thị Quỳnh",  "quynh.hv@gmail.com",    "0990123458", new DateTime(2001,8,28),  "Nữ",  "11 Đội Cấn, Ba Đình, Hà Nội",                   "Sơ cấp",    "HV017", new DateTime(2026,2,15)),
            ("Lưu Văn Sơn",      "son.hv@gmail.com",      "0901234569", new DateTime(1996,6,11),  "Nam", "55 Giảng Võ, Ba Đình, Hà Nội",                  "Trung cấp", "HV018", new DateTime(2026,2,20)),
            ("Đặng Thị Thủy",    "thuy.hv@gmail.com",     "0912345680", new DateTime(2002,11,4),  "Nữ",  "38 Phạm Ngọc Thạch, Đống Đa, Hà Nội",          "Sơ cấp",    "HV019", new DateTime(2026,2,28)),
            ("Hồ Văn Tuấn",      "tuan.hv@gmail.com",     "0923456791", new DateTime(1994,1,17),  "Nam", "74 Xã Đàn, Đống Đa, Hà Nội",                   "Nâng cao",  "HV020", new DateTime(2026,3,1)),
        };

        var hvUsers = hvRawData.Select(h => new NguoiDung
        {
            HoTen=h.Ten, Email=h.Email, MatKhauHash=hvPass, SoDienThoai=h.Phone, VaiTro=VaiTro.User
        }).ToArray();
        context.NguoiDungs.AddRange(hvUsers);
        context.SaveChanges();

        var hocViens = hvRawData.Select((h, i) => new HocVien
        {
            NguoiDungId=hvUsers[i].Id, MaHocVien=h.Ma,
            NgaySinh=h.NgaySinh, GioiTinh=h.GioiTinh, DiaChi=h.DiaChi,
            TrinhDoHienTai=h.TrinhDo, NgayDangKy=h.NgayDK
        }).ToArray();
        context.HocViens.AddRange(hocViens);
        context.SaveChanges();

        // ─── 8 Khóa học ──────────────────────────────────────────────────
        // [0,1,3] = DaKetThuc  [2,4,5] = DangHoc  [6,7] = MoChuaHoc
        var khoaHocs = new[]
        {
            new KhoaHoc { MaKhoaHoc="TA-CB-01",    TenKhoaHoc="Tiếng Anh Căn Bản A1",      MoTa="Khóa học dành cho người mới bắt đầu, xây dựng nền tảng vững chắc về ngữ pháp và từ vựng cơ bản.",       NgonNgu="Tiếng Anh",   TrinhDo="Căn bản",   SoBuoiHoc=24, ThoiLuongMoiBuoi=90,  HocPhi=2500000, SoLuongToiDa=20, TrangThai=TrangThaiKhoaHoc.DaKetThuc, NgayBatDau=new DateTime(2025,9,1),  NgayKetThuc=new DateTime(2025,12,31) },
            new KhoaHoc { MaKhoaHoc="TA-GT-02",    TenKhoaHoc="Tiếng Anh Giao Tiếp B1",    MoTa="Phát triển kỹ năng nói và nghe trong các tình huống giao tiếp hàng ngày và công việc thực tế.",          NgonNgu="Tiếng Anh",   TrinhDo="Trung cấp", SoBuoiHoc=28, ThoiLuongMoiBuoi=90,  HocPhi=3200000, SoLuongToiDa=15, TrangThai=TrangThaiKhoaHoc.DaKetThuc, NgayBatDau=new DateTime(2026,1,15), NgayKetThuc=new DateTime(2026,4,30) },
            new KhoaHoc { MaKhoaHoc="TA-IELTS-03", TenKhoaHoc="Luyện Thi IELTS 6.5+",     MoTa="Khóa luyện thi IELTS toàn diện, mục tiêu đạt band 6.5 trở lên với đầy đủ 4 kỹ năng Nghe Nói Đọc Viết.", NgonNgu="Tiếng Anh",   TrinhDo="Nâng cao",  SoBuoiHoc=32, ThoiLuongMoiBuoi=120, HocPhi=5500000, SoLuongToiDa=12, TrangThai=TrangThaiKhoaHoc.DangHoc,   NgayBatDau=new DateTime(2026,3,1),  NgayKetThuc=new DateTime(2026,7,31) },
            new KhoaHoc { MaKhoaHoc="TN-N5-01",    TenKhoaHoc="Tiếng Nhật Căn Bản N5",     MoTa="Khóa học tiếng Nhật dành cho người mới bắt đầu, học Hiragana/Katakana và hướng tới chứng chỉ JLPT N5.", NgonNgu="Tiếng Nhật",  TrinhDo="Căn bản",   SoBuoiHoc=30, ThoiLuongMoiBuoi=90,  HocPhi=3000000, SoLuongToiDa=18, TrangThai=TrangThaiKhoaHoc.DaKetThuc, NgayBatDau=new DateTime(2025,11,1), NgayKetThuc=new DateTime(2026,2,28) },
            new KhoaHoc { MaKhoaHoc="TN-N4-02",    TenKhoaHoc="Tiếng Nhật Nâng Cao N4",    MoTa="Nâng cao tiếng Nhật từ trình độ N5 lên N4, mở rộng ngữ pháp và vốn từ vựng giao tiếp tự nhiên.",       NgonNgu="Tiếng Nhật",  TrinhDo="Trung cấp", SoBuoiHoc=30, ThoiLuongMoiBuoi=90,  HocPhi=3500000, SoLuongToiDa=15, TrangThai=TrangThaiKhoaHoc.DangHoc,   NgayBatDau=new DateTime(2026,3,1),  NgayKetThuc=new DateTime(2026,6,30) },
            new KhoaHoc { MaKhoaHoc="TH-CB-01",    TenKhoaHoc="Tiếng Hàn Căn Bản TOPIK 1", MoTa="Khóa học tiếng Hàn cho người mới bắt đầu, học Hangul và giao tiếp cơ bản, hướng tới chứng chỉ TOPIK 1.",NgonNgu="Tiếng Hàn",   TrinhDo="Căn bản",   SoBuoiHoc=24, ThoiLuongMoiBuoi=90,  HocPhi=2800000, SoLuongToiDa=18, TrangThai=TrangThaiKhoaHoc.DangHoc,   NgayBatDau=new DateTime(2026,2,1),  NgayKetThuc=new DateTime(2026,5,31) },
            new KhoaHoc { MaKhoaHoc="TT-GT-01",    TenKhoaHoc="Tiếng Trung Giao Tiếp",     MoTa="Khóa học tiếng Trung ứng dụng giao tiếp thực tế và thương mại, phù hợp với người đi làm.",              NgonNgu="Tiếng Trung", TrinhDo="Trung cấp", SoBuoiHoc=28, ThoiLuongMoiBuoi=90,  HocPhi=3000000, SoLuongToiDa=16, TrangThai=TrangThaiKhoaHoc.MoChuaHoc, NgayBatDau=new DateTime(2026,6,1),  NgayKetThuc=new DateTime(2026,9,30)  },
            new KhoaHoc { MaKhoaHoc="TP-CB-01",    TenKhoaHoc="Tiếng Pháp Căn Bản A1",     MoTa="Khóa học tiếng Pháp cho người mới bắt đầu, xây dựng nền tảng từ vựng, ngữ pháp và phát âm chuẩn.",     NgonNgu="Tiếng Pháp",  TrinhDo="Căn bản",   SoBuoiHoc=24, ThoiLuongMoiBuoi=90,  HocPhi=2600000, SoLuongToiDa=15, TrangThai=TrangThaiKhoaHoc.MoChuaHoc, NgayBatDau=new DateTime(2026,7,1),  NgayKetThuc=new DateTime(2026,10,31) },
        };
        context.KhoaHocs.AddRange(khoaHocs);
        context.SaveChanges();

        // ─── Lịch học ────────────────────────────────────────────────────
        var lichHocs = new List<LichHoc>
        {
            // kh[0] TA-CB-01: T2/T4/T6 8:00-9:30
            new() { KhoaHocId=khoaHocs[0].Id, ThuTrongTuan=DayOfWeek.Monday,    GioBatDau=new TimeSpan(8,0,0),  GioKetThuc=new TimeSpan(9,30,0),  PhongHoc="P.101", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/ta-cb-01" },
            new() { KhoaHocId=khoaHocs[0].Id, ThuTrongTuan=DayOfWeek.Wednesday,  GioBatDau=new TimeSpan(8,0,0),  GioKetThuc=new TimeSpan(9,30,0),  PhongHoc="P.101", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/ta-cb-01" },
            new() { KhoaHocId=khoaHocs[0].Id, ThuTrongTuan=DayOfWeek.Friday,     GioBatDau=new TimeSpan(8,0,0),  GioKetThuc=new TimeSpan(9,30,0),  PhongHoc="P.101", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/ta-cb-01" },
            // kh[1] TA-GT-02: T2/T5 18:00-19:30
            new() { KhoaHocId=khoaHocs[1].Id, ThuTrongTuan=DayOfWeek.Monday,    GioBatDau=new TimeSpan(18,0,0), GioKetThuc=new TimeSpan(19,30,0), PhongHoc="P.203", HinhThuc=HinhThucHoc.Offline },
            new() { KhoaHocId=khoaHocs[1].Id, ThuTrongTuan=DayOfWeek.Thursday,   GioBatDau=new TimeSpan(18,0,0), GioKetThuc=new TimeSpan(19,30,0), PhongHoc="P.203", HinhThuc=HinhThucHoc.Offline },
            // kh[2] TA-IELTS-03: T3/T6 19:00-21:00
            new() { KhoaHocId=khoaHocs[2].Id, ThuTrongTuan=DayOfWeek.Tuesday,   GioBatDau=new TimeSpan(19,0,0), GioKetThuc=new TimeSpan(21,0,0),  PhongHoc="P.301", HinhThuc=HinhThucHoc.Offline },
            new() { KhoaHocId=khoaHocs[2].Id, ThuTrongTuan=DayOfWeek.Friday,     GioBatDau=new TimeSpan(19,0,0), GioKetThuc=new TimeSpan(21,0,0),  PhongHoc="P.301", HinhThuc=HinhThucHoc.Offline },
            // kh[3] TN-N5-01: T7/CN 9:00-10:30
            new() { KhoaHocId=khoaHocs[3].Id, ThuTrongTuan=DayOfWeek.Saturday,   GioBatDau=new TimeSpan(9,0,0),  GioKetThuc=new TimeSpan(10,30,0), PhongHoc="P.102", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/tn-n5-01" },
            new() { KhoaHocId=khoaHocs[3].Id, ThuTrongTuan=DayOfWeek.Sunday,     GioBatDau=new TimeSpan(9,0,0),  GioKetThuc=new TimeSpan(10,30,0), PhongHoc="P.102", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/tn-n5-01" },
            // kh[4] TN-N4-02: T3/T6 18:00-19:30
            new() { KhoaHocId=khoaHocs[4].Id, ThuTrongTuan=DayOfWeek.Tuesday,   GioBatDau=new TimeSpan(18,0,0), GioKetThuc=new TimeSpan(19,30,0), PhongHoc="P.201", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/tn-n4-02" },
            new() { KhoaHocId=khoaHocs[4].Id, ThuTrongTuan=DayOfWeek.Friday,     GioBatDau=new TimeSpan(18,0,0), GioKetThuc=new TimeSpan(19,30,0), PhongHoc="P.201", HinhThuc=HinhThucHoc.KetHop,  LinkMeetMacDinh="https://meet.google.com/tn-n4-02" },
            // kh[5] TH-CB-01: T2/T4 17:00-18:30
            new() { KhoaHocId=khoaHocs[5].Id, ThuTrongTuan=DayOfWeek.Monday,    GioBatDau=new TimeSpan(17,0,0), GioKetThuc=new TimeSpan(18,30,0), PhongHoc="P.202", HinhThuc=HinhThucHoc.Offline },
            new() { KhoaHocId=khoaHocs[5].Id, ThuTrongTuan=DayOfWeek.Wednesday,  GioBatDau=new TimeSpan(17,0,0), GioKetThuc=new TimeSpan(18,30,0), PhongHoc="P.202", HinhThuc=HinhThucHoc.Offline },
            // kh[6] TT-GT-01: T3/T5 18:00-19:30
            new() { KhoaHocId=khoaHocs[6].Id, ThuTrongTuan=DayOfWeek.Tuesday,   GioBatDau=new TimeSpan(18,0,0), GioKetThuc=new TimeSpan(19,30,0), PhongHoc="P.302", HinhThuc=HinhThucHoc.Offline },
            new() { KhoaHocId=khoaHocs[6].Id, ThuTrongTuan=DayOfWeek.Thursday,   GioBatDau=new TimeSpan(18,0,0), GioKetThuc=new TimeSpan(19,30,0), PhongHoc="P.302", HinhThuc=HinhThucHoc.Offline },
            // kh[7] TP-CB-01: T7 9:00-10:30
            new() { KhoaHocId=khoaHocs[7].Id, ThuTrongTuan=DayOfWeek.Saturday,   GioBatDau=new TimeSpan(9,0,0),  GioKetThuc=new TimeSpan(10,30,0), PhongHoc="P.103", HinhThuc=HinhThucHoc.Offline },
        };
        context.LichHocs.AddRange(lichHocs);
        context.SaveChanges();

        // ─── Phân công ───────────────────────────────────────────────────
        context.PhanCongs.AddRange(
            new PhanCong { GiangVienId=giangViens[0].Id, KhoaHocId=khoaHocs[0].Id },
            new PhanCong { GiangVienId=giangViens[0].Id, KhoaHocId=khoaHocs[1].Id },
            new PhanCong { GiangVienId=giangViens[0].Id, KhoaHocId=khoaHocs[2].Id },
            new PhanCong { GiangVienId=giangViens[1].Id, KhoaHocId=khoaHocs[3].Id },
            new PhanCong { GiangVienId=giangViens[1].Id, KhoaHocId=khoaHocs[4].Id },
            new PhanCong { GiangVienId=giangViens[2].Id, KhoaHocId=khoaHocs[5].Id },
            new PhanCong { GiangVienId=giangViens[3].Id, KhoaHocId=khoaHocs[6].Id },
            new PhanCong { GiangVienId=giangViens[4].Id, KhoaHocId=khoaHocs[7].Id }
        );
        context.SaveChanges();

        // ─── Đăng ký ─────────────────────────────────────────────────────
        // kh[0] DaKetThuc: hv[0..7] (8 HV)
        // kh[1] DaKetThuc: hv[2..9] (8 HV)
        // kh[2] DangHoc  : hv[8,9,13,14,19] (5 HV)
        // kh[3] DaKetThuc: hv[4..11] (8 HV)
        // kh[4] DangHoc  : hv[11,12,15,16] (4 HV)
        // kh[5] DangHoc  : hv[1,5,10,14,17,18] (6 HV)
        // kh[6] MoChuaHoc: hv[3,7,13,17] (4 HV - ChoDuyet)
        // kh[7] MoChuaHoc: hv[6,15,19] (3 HV - ChoDuyet)
        var dangKys = new List<DangKy>();

        void AddDK(int hvIdx, int khIdx, TrangThaiDangKy ts, decimal paid, DateTime? payDate, DateTime? ngayDK = null)
            => dangKys.Add(new DangKy { HocVienId=hocViens[hvIdx].Id, KhoaHocId=khoaHocs[khIdx].Id, TrangThai=ts, SoTienDaThanhToan=paid, NgayThanhToan=payDate, NgayDangKy=ngayDK ?? hocViens[hvIdx].NgayDangKy });

        // kh[0]
        for (int i=0; i<8; i++) AddDK(i, 0, TrangThaiDangKy.HoanThanh, 2500000, new DateTime(2025,9,3), new DateTime(2025,9,3));
        // kh[1]
        for (int i=2; i<10; i++) AddDK(i, 1, TrangThaiDangKy.HoanThanh, 3200000, new DateTime(2026,1,17), new DateTime(2026,1,17));
        // kh[2] — including hv[0] (mai.hv)
        foreach (int i in new[]{0,8,9,13,14,19}) AddDK(i, 2, TrangThaiDangKy.DangHoc, 5500000, new DateTime(2026,2,25), new DateTime(2026,2,25));
        // kh[3]
        for (int i=4; i<12; i++) AddDK(i, 3, TrangThaiDangKy.HoanThanh, 3000000, new DateTime(2025,11,3), new DateTime(2025,11,3));
        // kh[4]
        foreach (int i in new[]{11,12,15,16}) AddDK(i, 4, TrangThaiDangKy.DangHoc, 3500000, new DateTime(2026,2,28), new DateTime(2026,2,28));
        // kh[5] — including hv[0] (mai.hv)
        foreach (int i in new[]{0,1,5,10,14,17,18}) AddDK(i, 5, TrangThaiDangKy.DangHoc, 2800000, new DateTime(2026,1,30), new DateTime(2026,1,30));
        // kh[6] (ChoDuyet)
        foreach (int i in new[]{3,7,13,17}) AddDK(i, 6, TrangThaiDangKy.ChoDuyet, 0, null, new DateTime(2026,4,15));
        // kh[7] (ChoDuyet)
        foreach (int i in new[]{6,15,19}) AddDK(i, 7, TrangThaiDangKy.ChoDuyet, 0, null, new DateTime(2026,4,20));

        context.DangKys.AddRange(dangKys);
        context.SaveChanges();

        // Build lookup: (HocVienId, KhoaHocId) → DangKy
        var dkMap = dangKys.ToDictionary(dk => (dk.HocVienId, dk.KhoaHocId));

        // ─── Buổi học ────────────────────────────────────────────────────
        var allBuoiHocs = new List<BuoiHoc>();
        var today = DateTime.Today;

        // kh[0]: T2/T4/T6 from 2025-09-01, 24 sessions (all completed)
        AddSessions(allBuoiHocs, khoaHocs[0].Id,
            new[] { (lichHocs[0], DayOfWeek.Monday), (lichHocs[1], DayOfWeek.Wednesday), (lichHocs[2], DayOfWeek.Friday) },
            new DateTime(2025,9,1), 24, new TimeSpan(8,0,0), new TimeSpan(9,30,0), "P.101", HinhThucHoc.KetHop, allDone: true);

        // kh[1]: T2/T5 from 2026-01-15, 28 sessions (all completed)
        AddSessions(allBuoiHocs, khoaHocs[1].Id,
            new[] { (lichHocs[3], DayOfWeek.Monday), (lichHocs[4], DayOfWeek.Thursday) },
            new DateTime(2026,1,15), 28, new TimeSpan(18,0,0), new TimeSpan(19,30,0), "P.203", HinhThucHoc.Offline, allDone: true);

        // kh[2]: T3/T6 from 2026-03-01, 16 sessions generated (some past)
        AddSessions(allBuoiHocs, khoaHocs[2].Id,
            new[] { (lichHocs[5], DayOfWeek.Tuesday), (lichHocs[6], DayOfWeek.Friday) },
            new DateTime(2026,3,1), 16, new TimeSpan(19,0,0), new TimeSpan(21,0,0), "P.301", HinhThucHoc.Offline, allDone: false);

        // kh[3]: T7/CN from 2025-11-01, 30 sessions (all completed)
        AddSessions(allBuoiHocs, khoaHocs[3].Id,
            new[] { (lichHocs[7], DayOfWeek.Saturday), (lichHocs[8], DayOfWeek.Sunday) },
            new DateTime(2025,11,1), 30, new TimeSpan(9,0,0), new TimeSpan(10,30,0), "P.102", HinhThucHoc.KetHop, allDone: true);

        // kh[4]: T3/T6 from 2026-03-01, 18 sessions generated
        AddSessions(allBuoiHocs, khoaHocs[4].Id,
            new[] { (lichHocs[9], DayOfWeek.Tuesday), (lichHocs[10], DayOfWeek.Friday) },
            new DateTime(2026,3,1), 18, new TimeSpan(18,0,0), new TimeSpan(19,30,0), "P.201", HinhThucHoc.KetHop, allDone: false);

        // kh[5]: T2/T4 from 2026-02-01, 20 sessions generated
        AddSessions(allBuoiHocs, khoaHocs[5].Id,
            new[] { (lichHocs[11], DayOfWeek.Monday), (lichHocs[12], DayOfWeek.Wednesday) },
            new DateTime(2026,2,1), 20, new TimeSpan(17,0,0), new TimeSpan(18,30,0), "P.202", HinhThucHoc.Offline, allDone: false);

        context.BuoiHocs.AddRange(allBuoiHocs);
        context.SaveChanges();

        // ─── Điểm danh ───────────────────────────────────────────────────
        var diemDanhs = new List<DiemDanh>();

        // Helpers: 0=CoMat 1=Vang 2=VangCoPhep 3=HocOnline
        void AddDiemDanh(BuoiHoc bh, int dangKyId, int code) => diemDanhs.Add(new DiemDanh
        {
            BuoiHocId=bh.Id, DangKyId=dangKyId,
            TrangThai=code switch { 1=>TrangThaiDiemDanh.Vang, 2=>TrangThaiDiemDanh.VangCoPhep, 3=>TrangThaiDiemDanh.HocOnline, _=>TrangThaiDiemDanh.CoMat },
            NgayDiemDanh=bh.NgayHoc.AddHours(10)
        });

        // kh[0] patterns (24 sessions, per student)
        var kh0Sessions = allBuoiHocs.Where(b => b.KhoaHocId==khoaHocs[0].Id && b.DaDienRa).OrderBy(b => b.SoBuoiThuTu).ToList();
        int[][] kh0Pat = {
            new[]{0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[0] 95%
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[1] 100%
            new[]{0,0,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[2] 2 vắng
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[3] 100%
            new[]{0,0,0,1,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0}, // hv[4] 3 vắng
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[5] 100%
            new[]{0,1,0,0,1,0,0,0,1,0,0,1,0,0,0,1,0,0,0,0,1,0,0,1}, // hv[6] 7 vắng (>20%!)
            new[]{0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[7] online 1 lần
        };
        for (int j=0; j<8; j++)
        {
            var dkId = dkMap[(hocViens[j].Id, khoaHocs[0].Id)].Id;
            for (int s=0; s<kh0Sessions.Count && s<kh0Pat[j].Length; s++)
                AddDiemDanh(kh0Sessions[s], dkId, kh0Pat[j][s]);
        }

        // kh[1] patterns (28 sessions)
        var kh1Sessions = allBuoiHocs.Where(b => b.KhoaHocId==khoaHocs[1].Id && b.DaDienRa).OrderBy(b => b.SoBuoiThuTu).ToList();
        int[][] kh1Pat = {
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[2]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[3]
            new[]{0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[4]
            new[]{0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[5]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[6]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[7]
            new[]{0,2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[8]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[9]
        };
        int[] kh1Idxs = {2,3,4,5,6,7,8,9};
        for (int j=0; j<8; j++)
        {
            var dkId = dkMap[(hocViens[kh1Idxs[j]].Id, khoaHocs[1].Id)].Id;
            for (int s=0; s<kh1Sessions.Count && s<kh1Pat[j].Length; s++)
                AddDiemDanh(kh1Sessions[s], dkId, kh1Pat[j][s]);
        }

        // kh[3] patterns (30 sessions)
        var kh3Sessions = allBuoiHocs.Where(b => b.KhoaHocId==khoaHocs[3].Id && b.DaDienRa).OrderBy(b => b.SoBuoiThuTu).ToList();
        int[][] kh3Pat = {
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[4]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[5]
            new[]{0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[6]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[7]
            new[]{0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[8]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[9]
            new[]{0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[10]
            new[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // hv[11]
        };
        int[] kh3Idxs = {4,5,6,7,8,9,10,11};
        for (int j=0; j<8; j++)
        {
            var dkId = dkMap[(hocViens[kh3Idxs[j]].Id, khoaHocs[3].Id)].Id;
            for (int s=0; s<kh3Sessions.Count && s<kh3Pat[j].Length; s++)
                AddDiemDanh(kh3Sessions[s], dkId, kh3Pat[j][s]);
        }

        // kh[2], kh[4], kh[5] — tất cả CoMat cho các buổi đã diễn ra
        foreach (var bh in allBuoiHocs.Where(b => b.KhoaHocId==khoaHocs[2].Id && b.DaDienRa))
            foreach (int i in new[]{0,8,9,13,14,19})
                AddDiemDanh(bh, dkMap[(hocViens[i].Id, khoaHocs[2].Id)].Id, 0);

        foreach (var bh in allBuoiHocs.Where(b => b.KhoaHocId==khoaHocs[4].Id && b.DaDienRa))
            foreach (int i in new[]{11,12,15,16})
                AddDiemDanh(bh, dkMap[(hocViens[i].Id, khoaHocs[4].Id)].Id, 0);

        foreach (var bh in allBuoiHocs.Where(b => b.KhoaHocId==khoaHocs[5].Id && b.DaDienRa))
            foreach (int i in new[]{0,1,5,10,14,17,18})
                AddDiemDanh(bh, dkMap[(hocViens[i].Id, khoaHocs[5].Id)].Id, 0);

        context.DiemDanhs.AddRange(diemDanhs);
        context.SaveChanges();

        // ─── Điểm số ─────────────────────────────────────────────────────
        // Công thức: DiemTongKet = CC*1 + GK*3 + CK*6  (thang 0-100)
        var diemList = new List<Diem>();

        Diem MakeDiem(int dangKyId, decimal cc, decimal gk, decimal ck, string nhanXet, DateTime ngayCapNhat)
        {
            var tk = Math.Round(cc*1m + gk*3m + ck*6m, 2);
            return new Diem
            {
                DangKyId=dangKyId, DiemChuyenCan=cc, DiemGiuaKy=gk, DiemCuoiKy=ck,
                DiemTongKet=tk, DiemTrungBinh=tk,
                XepLoai=tk>=90?"Xuất sắc":tk>=80?"Giỏi":tk>=65?"Khá":tk>=50?"Trung bình":"Không đạt",
                DuDieuKienCapChungChi=tk>=50, DaHoanThanh=true,
                NhanXet=nhanXet, NgayCapNhat=ngayCapNhat
            };
        }

        // kh[0] grades — 8 học viên (hv[0..7]), hoàn thành
        var kh0Grades = new (decimal cc, decimal gk, decimal ck, string nx)[]
        {
            (9.0m, 7.5m, 8.0m, "Học viên tiến bộ tốt, cần cải thiện kỹ năng nói."),
            (10m,  8.0m, 9.0m, "Xuất sắc! Rất chăm chỉ và học tốt mọi kỹ năng."),
            (8.0m, 6.0m, 7.0m, "Cần cố gắng thêm về ngữ pháp và từ vựng."),
            (9.0m, 9.0m, 9.0m, "Học viên xuất sắc, kết quả đồng đều ở mọi kỹ năng."),
            (7.0m, 5.0m, 6.0m, "Cần ôn luyện thêm từ vựng và kỹ năng nghe."),
            (10m,  8.5m, 9.5m, "Tiến bộ vượt bậc, xứng đáng với nỗ lực của em."),
            (6.0m, 4.0m, 5.0m, "Cần nỗ lực nhiều hơn để đạt tiêu chuẩn khoá học."),
            (9.0m, 7.0m, 8.5m, "Học đều và nghiêm túc, tiếp tục phát huy nhé."),
        };
        for (int i=0; i<8; i++)
        {
            var g = kh0Grades[i];
            diemList.Add(MakeDiem(dkMap[(hocViens[i].Id, khoaHocs[0].Id)].Id, g.cc, g.gk, g.ck, g.nx, new DateTime(2026,1,5)));
        }

        // kh[1] grades — 8 học viên (hv[2..9]), hoàn thành
        var kh1Grades = new (decimal cc, decimal gk, decimal ck, string nx)[]
        {
            (9.0m, 7.0m, 8.0m, "Học viên chăm chỉ, kỹ năng giao tiếp ngày càng tốt."),
            (10m,  9.0m, 9.5m, "Xuất sắc! Kỹ năng nói rất tự nhiên và lưu loát."),
            (8.0m, 7.5m, 8.0m, "Tiến bộ đều, cần mạnh dạn hơn khi giao tiếp thực tế."),
            (7.0m, 6.0m, 6.5m, "Cần luyện thêm kỹ năng nghe và phát âm."),
            (9.0m, 8.0m, 8.5m, "Rất nỗ lực, kết quả tốt và đáng khen ngợi."),
            (10m,  9.5m, 9.5m, "Tài năng thiên bẩm về ngoại ngữ, xuất sắc toàn diện."),
            (6.0m, 5.0m, 5.5m, "Cần cải thiện thêm để đạt mức tốt hơn."),
            (8.0m, 7.0m, 7.5m, "Học đều đặn, khả năng giao tiếp khá tốt."),
        };
        int[] kh1HvIdx = {2,3,4,5,6,7,8,9};
        for (int i=0; i<8; i++)
        {
            var g = kh1Grades[i];
            diemList.Add(MakeDiem(dkMap[(hocViens[kh1HvIdx[i]].Id, khoaHocs[1].Id)].Id, g.cc, g.gk, g.ck, g.nx, new DateTime(2026,5,2)));
        }

        // kh[3] grades — 8 học viên (hv[4..11]), hoàn thành
        var kh3Grades = new (decimal cc, decimal gk, decimal ck, string nx)[]
        {
            (9.0m, 8.0m, 8.5m, "Học tiếng Nhật rất nghiêm túc, phát âm chuẩn."),
            (10m,  9.0m, 9.0m, "Xuất sắc! Từ vựng phong phú và ngữ pháp vững."),
            (8.0m, 7.0m, 7.5m, "Tiến bộ tốt, cần luyện thêm Kanji."),
            (9.0m, 8.5m, 9.0m, "Rất giỏi, đã sẵn sàng tiếp tục học N4."),
            (7.0m, 6.0m, 6.5m, "Nỗ lực tốt, cần cải thiện kỹ năng nghe hiểu."),
            (10m,  9.5m, 10m,  "Hoàn hảo! Sẵn sàng thi JLPT N5 bất cứ lúc nào."),
            (6.0m, 5.0m, 5.5m, "Cần ôn luyện thêm trước khi thi N5."),
            (8.0m, 7.5m, 8.0m, "Học đều đặn, kết quả tốt và ổn định."),
        };
        int[] kh3HvIdx = {4,5,6,7,8,9,10,11};
        for (int i=0; i<8; i++)
        {
            var g = kh3Grades[i];
            diemList.Add(MakeDiem(dkMap[(hocViens[kh3HvIdx[i]].Id, khoaHocs[3].Id)].Id, g.cc, g.gk, g.ck, g.nx, new DateTime(2026,3,3)));
        }

        // kh[2] grades — chỉ có giữa kỳ (đang học) — gồm hv[0] mai.hv
        var kh2GkScores = new[] { 9.0m, 8.5m, 8.0m, 9.0m, 8.5m, 7.5m };
        int[] kh2HvIdx = {0,8,9,13,14,19};
        for (int i=0; i<6; i++)
            diemList.Add(new Diem { DangKyId=dkMap[(hocViens[kh2HvIdx[i]].Id, khoaHocs[2].Id)].Id, DiemChuyenCan=10m, DiemGiuaKy=kh2GkScores[i], NhanXet="" });

        // kh[4] grades — chỉ có giữa kỳ (đang học)
        var kh4GkScores = new[] { 8.0m, 7.0m, 8.5m, 7.5m };
        int[] kh4HvIdx = {11,12,15,16};
        for (int i=0; i<4; i++)
            diemList.Add(new Diem { DangKyId=dkMap[(hocViens[kh4HvIdx[i]].Id, khoaHocs[4].Id)].Id, DiemChuyenCan=9.0m, DiemGiuaKy=kh4GkScores[i], NhanXet="" });

        // kh[5] grades — chưa có điểm (đang học) — gồm hv[0] mai.hv
        int[] kh5HvIdx = {0,1,5,10,14,17,18};
        foreach (int i in kh5HvIdx)
            diemList.Add(new Diem { DangKyId=dkMap[(hocViens[i].Id, khoaHocs[5].Id)].Id, DiemChuyenCan=10m, NhanXet="" });

        context.Diems.AddRange(diemList);

        // ─── Banners ─────────────────────────────────────────────────────
        context.Banners.AddRange(
            new Banner
            {
                TieuDe="Chào Mừng Đến Với EduCenter!",
                MoTa="Trung tâm ngoại ngữ hàng đầu Việt Nam – Khơi dậy tiềm năng ngôn ngữ của bạn với đội ngũ giáo viên chuyên nghiệp.",
                HinhAnh="https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=1200&h=400&fit=crop",
                DuongDanLienKet="/khoa-hoc",
                ThuTu=1, DangHienThi=true, NgayTao=new DateTime(2026,1,1)
            },
            new Banner
            {
                TieuDe="Luyện Thi IELTS 6.5+ – Đăng Ký Ngay!",
                MoTa="Khoá học IELTS chuyên sâu 4 kỹ năng, học phí ưu đãi 10% cho 20 học viên đầu tiên đăng ký tháng 6.",
                HinhAnh="https://images.unsplash.com/photo-1434030216411-0b793f4b4173?w=1200&h=400&fit=crop",
                DuongDanLienKet="/khoa-hoc",
                ThuTu=2, DangHienThi=true, NgayTao=new DateTime(2026,1,1)
            },
            new Banner
            {
                TieuDe="Tiếng Nhật – Cánh Cổng Đến Nhật Bản",
                MoTa="Học tiếng Nhật cùng giáo viên JLPT N1, lớp N5 và N4 đang tuyển sinh. Học phí hợp lý, lịch học linh hoạt.",
                HinhAnh="https://images.unsplash.com/photo-1528360983277-13d401cdc186?w=1200&h=400&fit=crop",
                DuongDanLienKet="/khoa-hoc",
                ThuTu=3, DangHienThi=true, NgayTao=new DateTime(2026,1,1)
            }
        );

        // ─── Bài viết ────────────────────────────────────────────────────
        context.BaiViets.AddRange(
            new BaiViet
            {
                TieuDe = "EduCenter khai giảng lớp IELTS 6.5+ tháng 3/2026",
                TomTat = "Khóa học IELTS chuyên sâu 4 kỹ năng, mục tiêu band 6.5+ với giáo viên kinh nghiệm 8 năm. Học phí ưu đãi 10% cho 20 học viên đầu tiên.",
                NoiDung = "EduCenter hân hạnh thông báo khai giảng khóa học Luyện Thi IELTS 6.5+ vào ngày 01/03/2026. Khóa học được thiết kế chuyên sâu với 32 buổi học, mỗi buổi 2 tiếng, tập trung vào cả 4 kỹ năng Nghe - Nói - Đọc - Viết theo chuẩn IELTS Academic.\n\nĐặc điểm nổi bật:\n- Giảng viên Nguyễn Thị Hương với 8 năm kinh nghiệm luyện thi IELTS\n- Tài liệu học tập cập nhật theo đề thi thực tế năm 2025-2026\n- Học kết hợp online/offline linh hoạt\n- Lớp học nhỏ tối đa 12 học viên để đảm bảo chất lượng\n\nĐăng ký ngay để nhận ưu đãi học phí 10% dành cho 20 học viên đầu tiên!",
                HinhAnh = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?w=800&h=400&fit=crop",
                TheLoai = "Sự kiện",
                TacGia = "Ban Truyền Thông EduCenter",
                DangHienThi = true, NoiBat = true, LuotXem = 142,
                NgayTao = new DateTime(2026, 2, 10), NguoiTaoId = 1
            },
            new BaiViet
            {
                TieuDe = "Tiếng Nhật – Ngôn ngữ của cơ hội nghề nghiệp tại Nhật Bản",
                TomTat = "Tại sao tiếng Nhật là lựa chọn thông minh cho người Việt? Tìm hiểu về thị trường lao động Nhật Bản và cơ hội nghề nghiệp hấp dẫn.",
                NoiDung = "Trong những năm gần đây, nhu cầu lao động người Việt tại Nhật Bản ngày càng tăng cao. Với chứng chỉ JLPT N3 trở lên, bạn có thể tiếp cận hàng nghìn cơ hội việc làm hấp dẫn tại xứ sở hoa anh đào.\n\nEduCenter cung cấp các khóa học tiếng Nhật từ N5 đến N2, giúp học viên:\n- Giao tiếp tự nhiên với người Nhật\n- Hiểu văn hóa và phong cách làm việc Nhật Bản\n- Chuẩn bị hồ sơ xin việc và phỏng vấn bằng tiếng Nhật\n\nLiên hệ với chúng tôi để được tư vấn miễn phí!",
                HinhAnh = "https://images.unsplash.com/photo-1528360983277-13d401cdc186?w=800&h=400&fit=crop",
                TheLoai = "Hướng dẫn",
                TacGia = "Trần Văn Minh – GV Tiếng Nhật",
                DangHienThi = true, NoiBat = false, LuotXem = 89,
                NgayTao = new DateTime(2026, 1, 20), NguoiTaoId = 1
            },
            new BaiViet
            {
                TieuDe = "Khuyến mãi Tết 2026: Giảm 15% tất cả các khóa học",
                TomTat = "Nhân dịp Tết Nguyên Đán 2026, EduCenter ưu đãi giảm 15% học phí cho tất cả các khóa học đăng ký từ ngày 15/01 đến 28/01/2026.",
                NoiDung = "EduCenter gửi lời chúc mừng năm mới 2026 đến toàn thể học viên và phụ huynh!\n\nNhân dịp Tết Nguyên Đán, chúng tôi tung chương trình khuyến mãi hấp dẫn:\n✅ Giảm 15% học phí cho tất cả các khóa học\n✅ Tặng bộ sách học kèm trị giá 200.000₫ cho 50 học viên đầu tiên\n✅ Miễn phí kiểm tra đầu vào\n\nThời gian áp dụng: 15/01/2026 – 28/01/2026\n\nĐừng bỏ lỡ cơ hội học ngoại ngữ với chi phí ưu đãi nhất trong năm!",
                HinhAnh = "https://images.unsplash.com/photo-1513151233558-d860c5398176?w=800&h=400&fit=crop",
                TheLoai = "Khuyến mãi",
                TacGia = "Ban Quản Lý EduCenter",
                DangHienThi = true, NoiBat = true, LuotXem = 267,
                NgayTao = new DateTime(2026, 1, 10), NguoiTaoId = 1
            },
            new BaiViet
            {
                TieuDe = "5 lý do nên học tiếng Hàn ngay hôm nay",
                TomTat = "K-Pop, K-Drama, văn hóa Hàn Quốc đang lan rộng toàn cầu. Đây là thời điểm hoàn hảo để bắt đầu học tiếng Hàn!",
                NoiDung = "1. Văn hóa Hàn Quốc đang \"viral\" toàn cầu\nVới sự bùng nổ của K-Pop và K-Drama, tiếng Hàn đang trở thành ngôn ngữ được yêu thích nhất thế giới.\n\n2. Cơ hội du học tại Hàn Quốc\nHàng trăm suất học bổng chính phủ và đại học dành cho sinh viên Việt Nam.\n\n3. Thị trường lao động hấp dẫn\nCác công ty Hàn Quốc (Samsung, LG, Hyundai) đang mở rộng tại Việt Nam và cần nhân sự biết tiếng Hàn.\n\n4. Ngôn ngữ khoa học, dễ học\nHangul có thể học trong 1 tuần, cấu trúc ngữ pháp có hệ thống.\n\n5. Đam mê được biến thành giá trị\nNếu bạn yêu thích K-Pop và K-Drama, tại sao không biến đam mê thành kỹ năng?",
                HinhAnh = "https://images.unsplash.com/photo-1517154421773-0529f29ea451?w=800&h=400&fit=crop",
                TheLoai = "Tin tức",
                TacGia = "Park Ji Young – GV Tiếng Hàn",
                DangHienThi = true, NoiBat = false, LuotXem = 203,
                NgayTao = new DateTime(2026, 1, 5), NguoiTaoId = 1
            }
        );

        context.SaveChanges();
    }

    // ─── Helper: sinh buổi học theo lịch ─────────────────────────────────
    private static void AddSessions(
        List<BuoiHoc> list, int khoaHocId,
        (LichHoc Lich, DayOfWeek Dow)[] schedule,
        DateTime startDate, int count,
        TimeSpan gioBD, TimeSpan gioKT,
        string phong, HinhThucHoc hinhThuc, bool allDone)
    {
        var today   = DateTime.Today;
        var current = startDate.Date;
        int generated = 0;
        int safety  = 0;

        while (generated < count && safety < count * 14)
        {
            safety++;
            var entry = schedule.FirstOrDefault(s => s.Dow == current.DayOfWeek);
            if (entry.Lich != null)
            {
                generated++;
                var isDone = allDone || current < today;
                list.Add(new BuoiHoc
                {
                    KhoaHocId   = khoaHocId,
                    LichHocId   = entry.Lich.Id,
                    NgayHoc     = current,
                    GioBatDau   = gioBD,
                    GioKetThuc  = gioKT,
                    PhongHoc    = phong,
                    HinhThuc    = hinhThuc,
                    LinkMeet    = hinhThuc != HinhThucHoc.Offline
                                    ? $"https://meet.google.com/kh{khoaHocId}-{generated:D3}" : string.Empty,
                    SoBuoiThuTu = generated,
                    DaDienRa    = isDone
                });
            }
            current = current.AddDays(1);
        }
    }
}
