using System.Net;
using QuanLyKhoaHoc.Tests.Helpers;

namespace QuanLyKhoaHoc.Tests;

/// <summary>
/// Tests that MVC pages return 200 OK without any server-side [Authorize].
/// All MVC controllers use client-side JS auth, not server-side [Authorize].
/// </summary>
public class MvcPageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MvcPageTests(CustomWebApplicationFactory factory)
    {
        // Don't follow redirects so we can catch unexpected 302s
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // --- Public pages (no auth needed) ---

    [Fact]
    public async Task Home_Index_Returns200()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Index_Returns200()
    {
        var response = await _client.GetAsync("/KhoaHoc");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GiangVien_Index_Returns200()
    {
        var response = await _client.GetAsync("/GiangVien");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Auth_DangNhap_Returns200()
    {
        var response = await _client.GetAsync("/Auth/DangNhap");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Auth_DangKy_Returns200()
    {
        var response = await _client.GetAsync("/Auth/DangKy");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- MVC pages that should NOT require server-side auth ---
    // (Auth handled client-side by JS)

    [Fact]
    public async Task Admin_Index_ReturnsOKWithoutAuth()
    {
        // Use /Admin/Index directly to avoid the /admin → /Admin/Index MapGet redirect (case-insensitive)
        var response = await _client.GetAsync("/Admin/Index");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}. " +
            $"Admin/Index should not have server-side [Authorize] - auth is handled by JS.");
    }

    [Fact]
    public async Task GiangVien_Dashboard_ReturnsOKWithoutAuth()
    {
        var response = await _client.GetAsync("/GiangVien/Dashboard");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}. " +
            $"GiangVien/Dashboard should not have server-side [Authorize].");
    }

    [Fact]
    public async Task GiangVien_HoSo_ReturnsOKWithoutAuth()
    {
        var response = await _client.GetAsync("/GiangVien/HoSo");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task HocVien_Dashboard_ReturnsOKWithoutAuth()
    {
        var response = await _client.GetAsync("/HocVien/Dashboard");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}. " +
            $"HocVien/Dashboard should not have server-side [Authorize].");
    }

    [Fact]
    public async Task HocVien_KetQua_ReturnsOKWithoutAuth()
    {
        var response = await _client.GetAsync("/HocVien/KetQua");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task HocVien_LichSuDangKy_ReturnsOKWithoutAuth()
    {
        var response = await _client.GetAsync("/HocVien/LichSuDangKy");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task HocVien_HoSo_ReturnsOKWithoutAuth()
    {
        var response = await _client.GetAsync("/HocVien/HoSo");
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but got {(int)response.StatusCode}.");
    }

    // --- Friendly URL redirects ---

    [Fact]
    public async Task FriendlyUrl_DangNhap_Redirects()
    {
        var response = await _client.GetAsync("/dang-nhap");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Auth/DangNhap", response.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task FriendlyUrl_DangKy_Redirects()
    {
        var response = await _client.GetAsync("/dang-ky");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task FriendlyUrl_Admin_Redirects()
    {
        var response = await _client.GetAsync("/admin");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin", response.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task FriendlyUrl_KhoaHoc_Redirects()
    {
        var response = await _client.GetAsync("/khoa-hoc");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task FriendlyUrl_GiangVien_Redirects()
    {
        var response = await _client.GetAsync("/giang-vien");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task FriendlyUrl_DiemCuaToi_Redirects()
    {
        var response = await _client.GetAsync("/diem-cua-toi");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task FriendlyUrl_HoSo_Redirects()
    {
        var response = await _client.GetAsync("/ho-so");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task FriendlyUrl_HoSoGiangVien_Redirects()
    {
        var response = await _client.GetAsync("/ho-so-giang-vien");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
