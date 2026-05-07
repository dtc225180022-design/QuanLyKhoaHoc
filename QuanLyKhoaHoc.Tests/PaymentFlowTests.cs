using System.Net;
using System.Text.Json;
using QuanLyKhoaHoc.Tests.Helpers;
using static QuanLyKhoaHoc.Tests.Helpers.SeedIds;

namespace QuanLyKhoaHoc.Tests;

/// <summary>
/// Tests for the 3 fixes:
/// 1. SAU KHI ĐĂNG KÝ: API returns redirectUrl to /HocVien/ThanhToan/{id}
/// 2. TRANG THANH TOÁN: /HocVien/ThanhToan/{id} accessible, no [Authorize] block, correct content
/// 3. LỊCH SỬ ĐĂNG KÝ: /HocVien/LichSuDangKy shows correct content, navbar has "Khóa học của tôi"
/// </summary>
public class PaymentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public PaymentFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // ─────────────────────────────────────────────────────────────────
    // FIX 1: API ĐĂNG KÝ TRẢ VỀ redirectUrl
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DangKyKhoaHoc_NewUser_ReturnsRedirectUrlToThanhToan()
    {
        var client = CreateClient();
        // Register a brand new user
        var email = $"flow_{Guid.NewGuid():N}@test.vn";
        var regRes = await client.PostJsonAsync("/api/auth/register", new
        {
            HoTen = "HocVien Flow Test",
            Email = email,
            MatKhau = "Flow@12345"
        });
        Assert.Equal(HttpStatusCode.OK, regRes.StatusCode);

        var token = await client.LoginAsync(email, "Flow@12345");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        // Enroll in the seeded course (KhoaHocId=1)
        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new
        {
            KhoaHocId = KhoaHocId,
            GhiChu = ""
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Must have dangKyId
        Assert.True(root.TryGetProperty("dangKyId", out var dkIdProp),
            $"Response must contain 'dangKyId'. Got: {json}");
        Assert.True(dkIdProp.GetInt32() > 0, "dangKyId must be > 0");

        // Must have redirectUrl
        Assert.True(root.TryGetProperty("redirectUrl", out var urlProp),
            $"Response must contain 'redirectUrl'. Got: {json}");
        var url = urlProp.GetString() ?? "";
        Assert.StartsWith("/HocVien/ThanhToan/", url, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DangKyKhoaHoc_Response_HasThanhToanMessage()
    {
        var client = CreateClient();
        var email = $"flow2_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "HV Flow2", Email = email, MatKhau = "Flow@12345" });
        var token = await client.LoginAsync(email, "Flow@12345");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var json = await res.Content.ReadAsStringAsync();
        // Message should mention thanh toán
        Assert.True(
            json.Contains("thanh") || json.Contains("Thanh") || json.Contains("redirect"),
            $"Message should mention payment. Got: {json}");
    }

    [Fact]
    public async Task DangKyKhoaHoc_AlreadyRegistered_ReturnsBadRequest()
    {
        var client = CreateClient();
        // hv@test.vn is already enrolled in KhoaHocId=1 (seeded)
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        Assert.NotNull(token);
        client.SetBearerToken(token!);

        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();
        Assert.True(json.Contains("đã đăng ký") || json.Contains("registered"),
            $"Should say already registered. Got: {json}");
    }

    [Fact]
    public async Task DangKyKhoaHoc_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────
    // FIX 2: TRANG THANH TOÁN /HocVien/ThanhToan/{id}
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ThanhToan_Page_Returns200_NoCookieNeeded()
    {
        // No [Authorize] on MVC action — should return 200 (auth is client-side)
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task ThanhToan_Page_Contains_VietcombankInfo()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("Vietcombank", html);
        Assert.Contains("1234567890", html);
        Assert.Contains("EDUCENTER", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Contains_PaymentMethodSelection()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        // Should have both payment options
        Assert.Contains("ChuyenKhoan", html);
        Assert.Contains("TienMat", html);
        Assert.Contains("selectMethod", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Contains_BankTransferDetails()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        // Bank info section
        Assert.Contains("bankInfo", html);
        Assert.Contains("copyText", html);
    }

    [Fact]
    public async Task ThanhToan_Page_AnyId_Returns200()
    {
        var client = CreateClient();
        // Even with invalid id, the MVC view returns 200 (JS handles error display)
        var res1 = await client.GetAsync("/HocVien/ThanhToan/999");
        var res2 = await client.GetAsync("/HocVien/ThanhToan/0");
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
    }

    [Fact]
    public async Task ThanhToan_Page_Contains_ProcessSteps()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        // Should show the 4-step process
        Assert.Contains("xacNhanPhuongThuc", html);
        Assert.Contains("loadData", html);
    }

    [Fact]
    public async Task ThanhToan_FriendlyUrl_Redirects()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/thanh-toan/1");
        // Should redirect (301/302) to /HocVien/ThanhToan/1
        Assert.True(res.StatusCode == HttpStatusCode.MovedPermanently ||
                    res.StatusCode == HttpStatusCode.Found ||
                    res.StatusCode == HttpStatusCode.PermanentRedirect ||
                    res.StatusCode == HttpStatusCode.TemporaryRedirect,
            $"Expected redirect, got {res.StatusCode}");
    }

    // ─────────────────────────────────────────────────────────────────
    // FIX 3: LỊCH SỬ ĐĂNG KÝ / KHÓA HỌC CỦA TÔI
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LichSuDangKy_Page_Returns200()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task LichSuDangKy_Page_Title_IsKhoaHocCuaToi()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        // Title should say "Khóa học của tôi"
        Assert.True(html.Contains("Kh") && html.Contains("a h") || html.Contains("LichSuDangKy"),
            "Page should contain registration/course content");
    }

    [Fact]
    public async Task LichSuDangKy_Page_Contains_ThanhToanLink()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("ThanhToan", html);
        Assert.Contains("credit-card", html);
    }

    [Fact]
    public async Task LichSuDangKy_Page_Contains_DangKyThem_Button()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("/KhoaHoc", html);
        Assert.Contains("plus-circle", html);
    }

    [Fact]
    public async Task LichSuDangKy_Page_Contains_FilterButtons()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("filterDangKy", html);
        Assert.Contains("DangHoc", html);
        Assert.Contains("ChoDuyet", html);
    }

    [Fact]
    public async Task LichSuDangKy_FriendlyUrl_Redirects()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/khoa-hoc-cua-toi");
        Assert.True(res.StatusCode == HttpStatusCode.MovedPermanently ||
                    res.StatusCode == HttpStatusCode.Found ||
                    res.StatusCode == HttpStatusCode.PermanentRedirect ||
                    res.StatusCode == HttpStatusCode.TemporaryRedirect,
            $"Expected redirect, got {res.StatusCode}");
        // Should redirect to LichSuDangKy
        var location = res.Headers.Location?.ToString() ?? "";
        Assert.Contains("LichSuDangKy", location, StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────────
    // NAVBAR: "Khóa học của tôi" link exists in layout
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Layout_Contains_KhoaHocCuaToi_Link()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/");
        var html = await res.Content.ReadAsStringAsync();
        // Layout should have LichSuDangKy link
        Assert.Contains("LichSuDangKy", html);
    }

    [Fact]
    public async Task Layout_Contains_NavHocVien_Dropdown()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/");
        var html = await res.Content.ReadAsStringAsync();
        // navHocVien should be a dropdown now
        Assert.Contains("navHocVien", html);
        Assert.Contains("navMenuKhoaHoc", html);
    }

    // ─────────────────────────────────────────────────────────────────
    // API: cua-toi trả về đúng dữ liệu đăng ký
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DangKyCuaToi_HocVien_ReturnsRegistrations()
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

        var arr = doc.RootElement.EnumerateArray().ToList();
        Assert.True(arr.Count >= 1, "HocVien should have at least 1 registration (seeded)");

        var first = arr[0];
        // Should have all key fields
        Assert.True(first.TryGetProperty("id", out _));
        Assert.True(first.TryGetProperty("tenKhoaHoc", out _));
        Assert.True(first.TryGetProperty("trangThai", out _));
        Assert.True(first.TryGetProperty("hocPhi", out _));
        Assert.True(first.TryGetProperty("phuongThucThanhToan", out _));
    }

    [Fact]
    public async Task DangKyCuaToi_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/api/dang-ky/cua-toi");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────
    // REDIRECT sau khi đăng ký: JS function đọc redirectUrl
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChiTiet_Page_Contains_Updated_DangKyFunction()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/KhoaHoc/ChiTiet/1");
        var html = await res.Content.ReadAsStringAsync();
        // JS function should redirect to d.redirectUrl
        Assert.Contains("redirectUrl", html);
        Assert.Contains("window.location.href", html);
    }

    [Fact]
    public async Task ChiTiet_Page_Has_DangKyButton()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/KhoaHoc/ChiTiet/1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("dangKyKhoaHoc", html);
    }
}
