using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add InMemory DB with unique name per factory instance
            var dbName = "TestDb_" + Guid.NewGuid();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Build the service provider and seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });

        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ApplicationDbContext db)
    {
        // Admin user
        var admin = new NguoiDung
        {
            Id = 1,
            HoTen = "Quản Trị Viên",
            Email = "admin@test.vn",
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            VaiTro = VaiTro.Admin,
            HoatDong = true
        };

        // GiangVien user
        var gvNguoiDung = new NguoiDung
        {
            Id = 2,
            HoTen = "Giảng Viên Test",
            Email = "gv@test.vn",
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Gv@123"),
            VaiTro = VaiTro.GiangVien,
            HoatDong = true
        };

        // HocVien user
        var hvNguoiDung = new NguoiDung
        {
            Id = 3,
            HoTen = "Học Viên Test",
            Email = "hv@test.vn",
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            VaiTro = VaiTro.User,
            HoatDong = true
        };

        db.NguoiDungs.AddRange(admin, gvNguoiDung, hvNguoiDung);
        db.SaveChanges();

        // GiangVien profile
        var gv = new GiangVien
        {
            Id = 1,
            NguoiDungId = 2,
            MaGiangVien = "GV001",
            ChuyenNganh = "Tiếng Anh",
            DangHoatDong = true
        };

        // HocVien profile
        var hv = new HocVien
        {
            Id = 1,
            NguoiDungId = 3,
            MaHocVien = "HV001"
        };

        db.GiangViens.Add(gv);
        db.HocViens.Add(hv);
        db.SaveChanges();

        // Enroll HocVien into KhoaHoc so hoc-vien endpoint has data
        // (saved after KhoaHoc below)

        // KhoaHoc
        var khoaHoc = new KhoaHoc
        {
            Id = 1,
            MaKhoaHoc = "KH001",
            TenKhoaHoc = "Tiếng Anh Cơ Bản",
            NgonNgu = "Tiếng Anh",
            TrinhDo = "Cơ bản",
            SoBuoiHoc = 20,
            ThoiLuongMoiBuoi = 90,
            HocPhi = 2000000,
            SoLuongToiDa = 30,
            TrangThai = TrangThaiKhoaHoc.MoChuaHoc,
            NgayBatDau = DateTime.Now.AddDays(30),
            NgayKetThuc = DateTime.Now.AddDays(120)
        };

        db.KhoaHocs.Add(khoaHoc);
        db.SaveChanges();

        // DangKy – enroll the test HocVien so hoc-vien endpoint returns data
        var dangKy = new DangKy
        {
            HocVienId = hv.Id,
            KhoaHocId = khoaHoc.Id,
            TrangThai = TrangThaiDangKy.DaDuyet,
            NgayDangKy = DateTime.Now
        };
        db.DangKys.Add(dangKy);

        // BaiViet – seed one visible & featured article for TinTuc tests
        var baiViet = new BaiViet
        {
            Id = SeedIds.BaiVietId,
            TieuDe = "Tin tức test nổi bật",
            TomTat = "Tóm tắt bài viết test",
            NoiDung = "Nội dung bài viết test đầy đủ.",
            TheLoai = "Tin tức",
            TacGia = "Admin Test",
            DangHienThi = true,
            NoiBat = true,
            NgayTao = DateTime.Now
        };
        db.BaiViets.Add(baiViet);
        db.SaveChanges();
    }
}

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Login and return JWT token</summary>
    public static async Task<string?> LoginAsync(this HttpClient client, string email, string password)
    {
        var payload = new StringContent(
            JsonSerializer.Serialize(new { Email = email, MatKhau = password }),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/auth/login", payload);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;
    }

    /// <summary>Set Authorization: Bearer {token} header</summary>
    public static void SetBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>POST with JSON body serialized via System.Text.Json</summary>
    public static async Task<HttpResponseMessage> PostJsonAsync(
        this HttpClient client, string url, object body)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");
        return await client.PostAsync(url, content);
    }

    /// <summary>PUT with JSON body serialized via System.Text.Json</summary>
    public static async Task<HttpResponseMessage> PutJsonAsync(
        this HttpClient client, string url, object body)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");
        return await client.PutAsync(url, content);
    }
}
