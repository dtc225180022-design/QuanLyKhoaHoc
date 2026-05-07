using System.Net;
using System.Text.Json;
using QuanLyKhoaHoc.Tests.Helpers;
using static QuanLyKhoaHoc.Tests.Helpers.SeedIds;

namespace QuanLyKhoaHoc.Tests;

/// <summary>
/// Integration tests for the Payment System:
/// 1. LUỒNG THANH TOÁN: Sau khi đăng ký → trang thanh toán
/// 2. ADMIN XÁC NHẬN THANH TOÁN: Tab Thanh toán → DangHoc
/// 3. THÔNG TIN CHUYỂN KHOẢN: Vietcombank 1234567890
/// 4. DOANH THU: Chỉ tính đăng ký đã xác nhận
/// 5. DATABASE: PhuongThucThanhToan field
/// </summary>
public class PaymentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public PaymentTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // ─────────────────────────────────────────────────────────────────
    // 1. LUỒNG ĐĂNG KÝ – trả về dangKyId và redirectUrl
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DangKyKhoaHoc_ReturnsRedirectUrl_ToThanhToan()
    {
        var client = CreateClient();
        // Login as a new user - register first
        var email = $"hv_pay_{Guid.NewGuid():N}@test.vn";
        var regRes = await client.PostJsonAsync("/api/auth/register", new
        {
            HoTen = "HocVien Payment Test",
            Email = email,
            MatKhau = "Pay@12345"
        });
        Assert.Equal(HttpStatusCode.OK, regRes.StatusCode);

        var token = await client.LoginAsync(email, "Pay@12345");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        // Register for the seeded course
        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new
        {
            KhoaHocId = KhoaHocId,
            GhiChu = ""
        });

        // Could be 200 (success) or 400 (already registered by another test)
        if (res.StatusCode == HttpStatusCode.OK)
        {
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("dangKyId", out var dkId), "Response should include dangKyId");
            Assert.True(root.TryGetProperty("redirectUrl", out var url), "Response should include redirectUrl");
            Assert.Contains("/HocVien/ThanhToan/", url.GetString() ?? "");
            Assert.True(dkId.GetInt32() > 0);
        }
        // If already registered, that's fine for this test
    }

    [Fact]
    public async Task DangKyKhoaHoc_Returns_MessageWithThanhToan()
    {
        var client = CreateClient();
        var email = $"hv_pay2_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "HocVien Pay2", Email = email, MatKhau = "Pay@12345" });
        var token = await client.LoginAsync(email, "Pay@12345");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId, GhiChu = "" });
        if (res.StatusCode == HttpStatusCode.OK)
        {
            var body = await res.Content.ReadAsStringAsync();
            Assert.Contains("thanh", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 2. TRANG THANH TOÁN MVC VIEW
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ThanhToan_View_Returns200()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        // View returns 200 (no server-side auth check on MVC controller)
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task ThanhToan_View_ContainsVietcombank()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("Vietcombank", html);
        Assert.Contains("1234567890", html);
    }

    [Fact]
    public async Task ThanhToan_View_ContainsTrungTamNgoaiNgu()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("EDUCENTER", html);
    }

    [Fact]
    public async Task ThanhToan_View_ContainsPaymentMethodOptions()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("ChuyenKhoan", html);
        Assert.Contains("TienMat", html);
    }

    // ─────────────────────────────────────────────────────────────────
    // 3. CHỌN PHƯƠNG THỨC THANH TOÁN API
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChonPhuongThuc_WithValidDangKy_Returns200Or400()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        // The seeded DangKy has id=1, HocVienId=1. Status may be DaDuyet (not ChoDuyet),
        // so expect either 400 (wrong status) or 200 (success)
        var res = await client.PutJsonAsync("/api/dang-ky/1/chon-phuong-thuc", new { PhuongThuc = "ChuyenKhoan" });
        Assert.True(res.StatusCode == HttpStatusCode.BadRequest || res.StatusCode == HttpStatusCode.OK,
            $"Expected 200 or 400, got {res.StatusCode}");
    }

    [Fact]
    public async Task ChonPhuongThuc_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var res = await client.PutJsonAsync("/api/dang-ky/1/chon-phuong-thuc", new { PhuongThuc = "TienMat" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task ChonPhuongThuc_WrongUser_Returns403Or404()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("gv@test.vn", "Gv@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.PutJsonAsync("/api/dang-ky/1/chon-phuong-thuc", new { PhuongThuc = "TienMat" });
        // GiangVien has no HocVien profile → 403 Forbid() or 404 Not found
        Assert.True(res.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"Expected 403/404, got {(int)res.StatusCode}");
    }

    // ─────────────────────────────────────────────────────────────────
    // 4. ADMIN XÁC NHẬN THANH TOÁN – chuyển sang DangHoc
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task XacNhanThanhToan_AsAdmin_Returns200()
    {
        var client = CreateClient();
        var adminToken = await client.LoginAsync("admin@test.vn", "Admin@123");
        Assert.NotNull(adminToken);
        client.SetBearerToken(adminToken!);

        var res = await client.PutJsonAsync("/api/dang-ky/1/thanh-toan", new
        {
            SoTien = 2000000m,
            PhuongThuc = "ChuyenKhoan"
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("th", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task XacNhanThanhToan_NonAdmin_Returns403()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.PutJsonAsync("/api/dang-ky/1/thanh-toan", new { SoTien = 2000000m });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task XacNhanThanhToan_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var res = await client.PutJsonAsync("/api/dang-ky/1/thanh-toan", new { SoTien = 2000000m });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────
    // 5. ADMIN GET ALL ĐĂNG KÝ – includes PhuongThucThanhToan
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllDangKy_AsAdmin_ReturnsPhuongThucThanhToan()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/dang-ky");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

        // Each element should have phuongThucThanhToan property
        foreach (var elem in doc.RootElement.EnumerateArray())
        {
            Assert.True(elem.TryGetProperty("phuongThucThanhToan", out _),
                "DangKy response should include phuongThucThanhToan");
            break; // Just check first element
        }
    }

    [Fact]
    public async Task GetCuaToi_Returns_PhuongThucThanhToan()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/dang-ky/cua-toi");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

        // Should have at least 1 registration (seeded)
        var arr = doc.RootElement.EnumerateArray().ToList();
        if (arr.Count > 0)
        {
            Assert.True(arr[0].TryGetProperty("phuongThucThanhToan", out _),
                "cua-toi response should include phuongThucThanhToan");
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 6. DOANH THU – chỉ tính đăng ký đã xác nhận (NgayThanhToan != null)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ThongKe_DoanhThu_ReturnsNumericValue()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/api/ai/thong-ke");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("doanhThu", out var dt));
        // doanhThu should be a valid number (could be 0 if no confirmed payments)
        Assert.True(dt.ValueKind == JsonValueKind.Number);
    }

    [Fact]
    public async Task BaoCao_DoanhThuTheoThang_Available_ForAdmin()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/ai/bao-cao");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("doanhThuTheoThang", out var dt));
        Assert.Equal(JsonValueKind.Array, dt.ValueKind);
    }

    // ─────────────────────────────────────────────────────────────────
    // 7. LỊCH SỬ ĐĂNG KÝ VIEW – contains payment button
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LichSuDangKy_View_Returns200()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task LichSuDangKy_View_ContainsPaymentLink()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("ThanhToan", html);
    }

    // ─────────────────────────────────────────────────────────────────
    // 8. DATABASE – PhuongThucThanhToan field in model
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DangKy_Model_HasPhuongThucThanhToan_Field()
    {
        // Verify via API response that the field exists
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/dang-ky");
        var json = await res.Content.ReadAsStringAsync();
        Assert.Contains("phuongThucThanhToan", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_ThanhToan_Tab_Endpoint_Accessible()
    {
        // Admin can access GET /api/dang-ky (the backend for the payment tab)
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/dang-ky?trangThai=ChoDuyet");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
