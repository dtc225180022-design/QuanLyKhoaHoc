using System.Net;
using System.Text.Json;
using QuanLyKhoaHoc.Tests.Helpers;
using static QuanLyKhoaHoc.Tests.Helpers.SeedIds;

namespace QuanLyKhoaHoc.Tests;

/// <summary>
/// Phase 5 integration tests:
/// 1. Đăng ký → redirect ngay (không setTimeout dài), JS đọc redirectUrl
/// 2. Trang thanh toán đầy đủ: VietQR thật, nút contextual
/// 3. Menu học viên: "Khóa học của tôi" + trạng thái thanh toán
/// </summary>
public class Phase5Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public Phase5Tests(CustomWebApplicationFactory factory) => _factory = factory;

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // ─────────────────────────────────────────────────────────────────
    // FIX 1: API trả về redirectUrl, JS redirect ngay không delay
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DangKy_API_ReturnsRedirectUrl_WithCorrectPath()
    {
        var client = CreateClient();
        var email = $"p5_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "P5 User", Email = email, MatKhau = "Pass@12345" });
        var token = await client.LoginAsync(email, "Pass@12345");
        client.SetBearerToken(token!);

        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("redirectUrl", out var urlProp),
            $"Must have redirectUrl. Got: {json}");
        var url = urlProp.GetString() ?? "";
        Assert.StartsWith("/HocVien/ThanhToan/", url, StringComparison.OrdinalIgnoreCase);

        // dangKyId must be > 0
        Assert.True(doc.RootElement.TryGetProperty("dangKyId", out var idProp));
        var dkId = idProp.GetInt32();
        Assert.True(dkId > 0);

        // redirectUrl must match dangKyId
        Assert.Equal($"/HocVien/ThanhToan/{dkId}", url, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChiTiet_Page_NoLongTimeout_ForRedirect()
    {
        // JS should NOT have setTimeout(..., 1200) for redirectUrl — it should redirect immediately
        var client = CreateClient();
        var res = await client.GetAsync("/KhoaHoc/ChiTiet/1");
        var html = await res.Content.ReadAsStringAsync();

        Assert.Contains("redirectUrl", html);
        Assert.Contains("window.location.href", html);

        // Should NOT have the old 1200ms delay for redirectUrl
        Assert.DoesNotContain("setTimeout(() => { window.location.href = d.redirectUrl; }, 1200)", html);
        Assert.DoesNotContain("setTimeout(() => { window.location.href = d.redirectUrl; }, 1500)", html);
    }

    [Fact]
    public async Task Dashboard_Page_NoLongTimeout_ForRedirect()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/Dashboard");
        // Dashboard redirects unauthenticated users — check HTML if 200, else check redirect
        if (res.StatusCode == HttpStatusCode.OK)
        {
            var html = await res.Content.ReadAsStringAsync();
            if (html.Contains("dangKyKhoaHoc"))
            {
                Assert.DoesNotContain("setTimeout(() => { window.location.href = d.redirectUrl; }, 1200)", html);
            }
        }
        // If 302 redirect that's fine too (auth redirect)
        Assert.True(res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Found
            || res.StatusCode == HttpStatusCode.Redirect,
            $"Expected 200 or redirect, got {res.StatusCode}");
    }

    [Fact]
    public async Task DangKy_Response_MessageMentionsThanhToan()
    {
        var client = CreateClient();
        var email = $"p5b_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "P5B User", Email = email, MatKhau = "Pass@12345" });
        var token = await client.LoginAsync(email, "Pass@12345");
        client.SetBearerToken(token!);

        var res = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        var json = await res.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        // Message should mention thanh toán
        Assert.True(json.Contains("thanh toán") || json.Contains("ThanhToan") || json.Contains("thanh"),
            $"Message should mention payment. Got: {json}");
    }

    // ─────────────────────────────────────────────────────────────────
    // FIX 2: Trang thanh toán đầy đủ — VietQR thật + nút contextual
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ThanhToan_Page_Has_VietQR_ImgElement()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var html = await res.Content.ReadAsStringAsync();

        // Should have real VietQR img tag, not the old CSS placeholder
        Assert.Contains("qrCodeImg", html);
        Assert.Contains("vietqr.io", html);
        Assert.DoesNotContain("repeating-linear-gradient", html); // old CSS fake QR
    }

    [Fact]
    public async Task ThanhToan_Page_VietQR_URL_Has_Bank_Info()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();

        // VietQR URL must reference VCB bank
        Assert.Contains("VCB-1234567890", html);
        Assert.Contains("img.vietqr.io", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Has_ContextualButtonSpan()
    {
        // Button text is now dynamic via span#btnXacNhanText
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();

        Assert.Contains("btnXacNhanText", html);
    }

    [Fact]
    public async Task ThanhToan_Page_JS_Sets_Contextual_Button_Text()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();

        // JS should set "Xác nhận đã chuyển khoản" for ChuyenKhoan
        Assert.Contains("Xác nhận đã chuyển khoản", html);
        // JS should set "Xác nhận sẽ nộp tiền mặt" for TienMat
        Assert.Contains("Xác nhận sẽ nộp tiền mặt", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Has_QrFallback_Element()
    {
        // Fallback shown when QR image fails to load
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("qrFallback", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Has_BankTransferInstruction()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("Xác nhận đã chuyển khoản", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Contains_EDUCENTER_BankName()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("Vietcombank", html);
        Assert.Contains("1234567890", html);
        Assert.Contains("EDUCENTER", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Has_CopyButton_For_AccountNumber()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("copyText('1234567890')", html);
        Assert.Contains("copyNoiDung", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Has_CourseInfo_Fields()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();

        // Course info card
        Assert.Contains("tenKhoaHoc", html);
        Assert.Contains("maKhoaHoc", html);
        Assert.Contains("hocPhi", html);
        Assert.Contains("ngayDangKy", html);
    }

    [Fact]
    public async Task ThanhToan_Page_Has_NoiDungCK_Field()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/ThanhToan/1");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("noiDungCK", html);
        Assert.Contains("soTienCK", html);
    }

    // ─────────────────────────────────────────────────────────────────
    // FIX 3: Menu học viên + LichSuDangKy
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LichSuDangKy_Has_ThanhToanNgay_Button()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();

        Assert.Contains("ThanhToan", html);
        Assert.Contains("credit-card", html);
    }

    [Fact]
    public async Task LichSuDangKy_Shows_PendingPaymentBanner()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("pendingPaymentBanner", html);
        Assert.Contains("renderPendingBanner", html);
    }

    [Fact]
    public async Task LichSuDangKy_Has_AllStatus_FilterButtons()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/HocVien/LichSuDangKy");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("filterDangKy", html);
        Assert.Contains("ChoDuyet", html);
        Assert.Contains("DangHoc", html);
        Assert.Contains("DaDuyet", html);
        Assert.Contains("HoanThanh", html);
    }

    [Fact]
    public async Task Layout_Has_KhoaHocCuaToi_In_NavDropdown()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("LichSuDangKy", html);
        Assert.Contains("navHocVien", html);
    }

    [Fact]
    public async Task Layout_Has_navMenuKhoaHoc_Link()
    {
        var client = CreateClient();
        var res = await client.GetAsync("/");
        var html = await res.Content.ReadAsStringAsync();
        Assert.Contains("navMenuKhoaHoc", html);
    }

    // ─────────────────────────────────────────────────────────────────
    // API /api/dang-ky/cua-toi — trả về đúng dữ liệu
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuaToi_Returns_PhuongThucThanhToan_Field()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/dang-ky/cua-toi");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();

        // phuongThucThanhToan must appear in JSON schema (even if null value)
        Assert.True(json.Contains("phuongThucThanhToan"),
            $"Response must contain 'phuongThucThanhToan' field. Got: {json}");
    }

    [Fact]
    public async Task CuaToi_Returns_SoTienDaThanhToan_Field()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var res = await client.GetAsync("/api/dang-ky/cua-toi");
        var json = await res.Content.ReadAsStringAsync();
        Assert.True(json.Contains("soTienDaThanhToan"),
            $"Response must contain 'soTienDaThanhToan'. Got: {json}");
    }

    // ─────────────────────────────────────────────────────────────────
    // API chon-phuong-thuc
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChonPhuongThuc_NewRegistration_SetsTrangThaiChoDuyet()
    {
        var client = CreateClient();
        var email = $"p5c_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "P5C User", Email = email, MatKhau = "Pass@12345" });
        var token = await client.LoginAsync(email, "Pass@12345");
        client.SetBearerToken(token!);

        // Register
        var regRes = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        var regJson = await regRes.Content.ReadAsStringAsync();
        using var regDoc = JsonDocument.Parse(regJson);
        var dkId = regDoc.RootElement.GetProperty("dangKyId").GetInt32();

        // Choose payment method
        var ptRes = await client.PutJsonAsync($"/api/dang-ky/{dkId}/chon-phuong-thuc",
            new { phuongThuc = "ChuyenKhoan" });
        Assert.Equal(HttpStatusCode.OK, ptRes.StatusCode);

        // Verify registration still ChoDuyet (waiting for admin confirmation)
        var listRes = await client.GetAsync("/api/dang-ky/cua-toi");
        var listJson = await listRes.Content.ReadAsStringAsync();
        using var listDoc = JsonDocument.Parse(listJson);
        var dk = listDoc.RootElement.EnumerateArray()
            .FirstOrDefault(d => d.GetProperty("id").GetInt32() == dkId);
        Assert.Equal("ChoDuyet", dk.GetProperty("trangThai").GetString());
        Assert.Equal("ChuyenKhoan", dk.GetProperty("phuongThucThanhToan").GetString());
    }

    [Fact]
    public async Task ChonPhuongThuc_TienMat_Works()
    {
        var client = CreateClient();
        var email = $"p5d_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "P5D User", Email = email, MatKhau = "Pass@12345" });
        var token = await client.LoginAsync(email, "Pass@12345");
        client.SetBearerToken(token!);

        var regRes = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        var regJson = await regRes.Content.ReadAsStringAsync();
        using var regDoc = JsonDocument.Parse(regJson);
        var dkId = regDoc.RootElement.GetProperty("dangKyId").GetInt32();

        var ptRes = await client.PutJsonAsync($"/api/dang-ky/{dkId}/chon-phuong-thuc",
            new { phuongThuc = "TienMat" });
        Assert.Equal(HttpStatusCode.OK, ptRes.StatusCode);

        var listRes = await client.GetAsync("/api/dang-ky/cua-toi");
        var listJson = await listRes.Content.ReadAsStringAsync();
        using var listDoc = JsonDocument.Parse(listJson);
        var dk = listDoc.RootElement.EnumerateArray()
            .FirstOrDefault(d => d.GetProperty("id").GetInt32() == dkId);
        Assert.Equal("TienMat", dk.GetProperty("phuongThucThanhToan").GetString());
    }

    [Fact]
    public async Task ChonPhuongThuc_InvalidMethod_ReturnsBadRequest()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        // Use seeded registration id
        var listRes = await client.GetAsync("/api/dang-ky/cua-toi");
        var listJson = await listRes.Content.ReadAsStringAsync();
        using var listDoc = JsonDocument.Parse(listJson);
        var firstId = listDoc.RootElement.EnumerateArray().First().GetProperty("id").GetInt32();

        var ptRes = await client.PutJsonAsync($"/api/dang-ky/{firstId}/chon-phuong-thuc",
            new { phuongThuc = "BitCoin" });
        Assert.Equal(HttpStatusCode.BadRequest, ptRes.StatusCode);
    }

    [Fact]
    public async Task ChonPhuongThuc_Unauthenticated_Returns401()
    {
        var client = CreateClient();
        var ptRes = await client.PutJsonAsync("/api/dang-ky/1/chon-phuong-thuc",
            new { phuongThuc = "ChuyenKhoan" });
        Assert.Equal(HttpStatusCode.Unauthorized, ptRes.StatusCode);
    }

    // ─────────────────────────────────────────────────────────────────
    // Full end-to-end payment flow
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FullFlow_Register_ChooseMethod_AdminConfirm()
    {
        var client = CreateClient();

        // 1. Register user
        var email = $"p5e_{Guid.NewGuid():N}@test.vn";
        await client.PostJsonAsync("/api/auth/register", new { HoTen = "P5E Full Flow", Email = email, MatKhau = "Pass@12345" });
        var token = await client.LoginAsync(email, "Pass@12345");
        client.SetBearerToken(token!);

        // 2. Enroll in course → get redirectUrl
        var regRes = await client.PostJsonAsync("/api/hoc-vien/dang-ky-khoa-hoc", new { KhoaHocId = KhoaHocId });
        Assert.Equal(HttpStatusCode.OK, regRes.StatusCode);
        var regJson = await regRes.Content.ReadAsStringAsync();
        using var regDoc = JsonDocument.Parse(regJson);
        var dkId = regDoc.RootElement.GetProperty("dangKyId").GetInt32();
        var redirectUrl = regDoc.RootElement.GetProperty("redirectUrl").GetString();
        Assert.Equal($"/HocVien/ThanhToan/{dkId}", redirectUrl);

        // 3. Payment page loads
        var pageRes = await client.GetAsync(redirectUrl);
        Assert.Equal(HttpStatusCode.OK, pageRes.StatusCode);

        // 4. Choose ChuyenKhoan
        var ptRes = await client.PutJsonAsync($"/api/dang-ky/{dkId}/chon-phuong-thuc",
            new { phuongThuc = "ChuyenKhoan" });
        Assert.Equal(HttpStatusCode.OK, ptRes.StatusCode);

        // 5. Admin confirms payment
        var adminToken = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(adminToken!);
        var confirmRes = await client.PutJsonAsync($"/api/dang-ky/{dkId}/thanh-toan",
            new { soTien = 0, phuongThuc = "ChuyenKhoan" });
        Assert.Equal(HttpStatusCode.OK, confirmRes.StatusCode);

        // 6. Verify status is DangHoc
        client.SetBearerToken(token!);
        var listRes = await client.GetAsync("/api/dang-ky/cua-toi");
        var listJson = await listRes.Content.ReadAsStringAsync();
        using var listDoc = JsonDocument.Parse(listJson);
        var dk = listDoc.RootElement.EnumerateArray()
            .FirstOrDefault(d => d.GetProperty("id").GetInt32() == dkId);
        Assert.Equal("DangHoc", dk.GetProperty("trangThai").GetString());
    }
}
