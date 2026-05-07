using System.Net;
using System.Text;
using System.Text.Json;
using QuanLyKhoaHoc.Tests.Helpers;

namespace QuanLyKhoaHoc.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithAdminCredentials_ReturnsTokenAndAdminRole()
    {
        // Act
        var token = await _client.LoginAsync("admin@test.vn", "Admin@123");

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Verify JWT role by calling profile endpoint
        _client.SetBearerToken(token!);
        var profileResp = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);

        var json = await profileResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var vaiTro = doc.RootElement.GetProperty("vaiTro").GetString();
        Assert.Equal("Admin", vaiTro);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Login_WithGiangVienCredentials_ReturnsTokenAndGiangVienRole()
    {
        var token = await _client.LoginAsync("gv@test.vn", "Gv@123");

        Assert.NotNull(token);
        _client.SetBearerToken(token!);

        var profileResp = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);

        var json = await profileResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("GiangVien", doc.RootElement.GetProperty("vaiTro").GetString());

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Login_WithHocVienCredentials_ReturnsTokenAndUserRole()
    {
        var token = await _client.LoginAsync("hv@test.vn", "User@123");

        Assert.NotNull(token);
        _client.SetBearerToken(token!);

        var profileResp = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);

        var json = await profileResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("User", doc.RootElement.GetProperty("vaiTro").GetString());

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var payload = new StringContent(
            JsonSerializer.Serialize(new { Email = "admin@test.vn", MatKhau = "WrongPassword" }),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var payload = new StringContent(
            JsonSerializer.Serialize(new { Email = "nobody@test.vn", MatKhau = "Password123" }),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithNewEmail_Returns200()
    {
        var payload = new StringContent(
            JsonSerializer.Serialize(new
            {
                HoTen = "New User",
                Email = "newuser@test.vn",
                MatKhau = "NewUser@123",
                SoDienThoai = "0900000000"
            }),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        // Register once
        var payload = new StringContent(
            JsonSerializer.Serialize(new
            {
                HoTen = "Dup User",
                Email = "dup@test.vn",
                MatKhau = "Dup@123",
                SoDienThoai = "0900000001"
            }),
            Encoding.UTF8,
            "application/json");
        await _client.PostAsync("/api/auth/register", payload);

        // Register again with same email
        var payload2 = new StringContent(
            JsonSerializer.Serialize(new
            {
                HoTen = "Dup User2",
                Email = "dup@test.vn",
                MatKhau = "Dup2@123",
                SoDienThoai = "0900000002"
            }),
            Encoding.UTF8,
            "application/json");
        var response = await _client.PostAsync("/api/auth/register", payload2);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
