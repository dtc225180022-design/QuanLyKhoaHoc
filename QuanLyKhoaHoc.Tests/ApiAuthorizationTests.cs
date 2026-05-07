using System.Net;
using System.Text;
using System.Text.Json;
using QuanLyKhoaHoc.Tests.Helpers;

namespace QuanLyKhoaHoc.Tests;

/// <summary>
/// Tests that API endpoints enforce role-based authorization correctly.
/// </summary>
public class ApiAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApiAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // --- Auth profile (requires any authenticated user) ---

    [Fact]
    public async Task Profile_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WithAdminToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WithHocVienToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- KhoaHoc API ---

    [Fact]
    public async Task KhoaHoc_GetAll_PublicAccess_Returns200()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/khoa-hoc");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Create_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var dto = CreateKhoaHocDto("KH999");
        var response = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Create_WithHocVienToken_Returns403()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var dto = CreateKhoaHocDto("KH998");
        var response = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Create_WithGiangVienToken_Returns403()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("gv@test.vn", "Gv@123");
        client.SetBearerToken(token!);

        var dto = CreateKhoaHocDto("KH997");
        var response = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Create_WithAdminToken_Returns201()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var dto = CreateKhoaHocDto("KH_NEW");
        var response = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Delete_WithAdminToken_Returns200()
    {
        // First create a course
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var dto = CreateKhoaHocDto("KH_DEL");
        var createResp = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(created);
        var id = doc.RootElement.GetProperty("id").GetInt32();

        // Delete it
        var deleteResp = await client.DeleteAsync($"/api/khoa-hoc/{id}");
        Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_Delete_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync("/api/khoa-hoc/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- HocVien API ---

    [Fact]
    public async Task HocVien_GetAll_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/hoc-vien");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HocVien_GetAll_WithHocVienToken_Returns403()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/hoc-vien");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HocVien_GetAll_WithAdminToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/hoc-vien");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HocVien_GetMyProfile_WithHocVienToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/hoc-vien/cua-toi");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HocVien_GetMyProfile_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/hoc-vien/cua-toi");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- DangKy API ---

    [Fact]
    public async Task DangKy_GetCuaToi_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/dang-ky/cua-toi");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DangKy_GetCuaToi_WithHocVienToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/dang-ky/cua-toi");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DangKy_GetAll_WithHocVienToken_Returns403()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/dang-ky");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DangKy_GetAll_WithAdminToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/dang-ky");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- AI/Stats API ---

    [Fact]
    public async Task AI_ThongKe_PublicAccess_Returns200()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/ai/thong-ke");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AI_BaoCao_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/ai/bao-cao");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AI_BaoCao_WithHocVienToken_Returns403()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/ai/bao-cao");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AI_BaoCao_WithAdminToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/ai/bao-cao");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AI_GoiYCuaToi_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/ai/goi-y-cua-toi");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AI_GoiYCuaToi_WithHocVienToken_Returns200()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/ai/goi-y-cua-toi");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- Helpers ---

    private static object CreateKhoaHocDto(string maKhoaHoc) => new
    {
        MaKhoaHoc = maKhoaHoc,
        TenKhoaHoc = "Test Khoa Hoc " + maKhoaHoc,
        MoTa = "Mo ta",
        NgonNgu = "Tiếng Anh",
        TrinhDo = "Cơ bản",
        SoBuoiHoc = 10,
        ThoiLuongMoiBuoi = 60,
        HocPhi = 1000000m,
        SoLuongToiDa = 20,
        NgayBatDau = DateTime.Now.AddDays(30),
        NgayKetThuc = DateTime.Now.AddDays(90),
        DanhSachLichHoc = new[]
        {
            new { ThuTrongTuan = 1, GioBatDau = "08:00", GioKetThuc = "09:30", PhongHoc = "P101" }
        }
    };
}
