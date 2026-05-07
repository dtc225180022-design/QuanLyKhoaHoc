using System.Net;
using System.Text;
using System.Text.Json;
using QuanLyKhoaHoc.Tests.Helpers;

namespace QuanLyKhoaHoc.Tests;

/// <summary>
/// Tests that API endpoints return correct data shapes and content.
/// </summary>
public class ApiDataTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ApiDataTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // --- KhoaHoc list ---

    [Fact]
    public async Task KhoaHoc_GetAll_ReturnsKhoaHocList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/khoa-hoc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

        // Seeded 1 KhoaHoc
        Assert.True(doc.RootElement.GetArrayLength() >= 1,
            "Expected at least 1 khoa hoc from seed data");
    }

    [Fact]
    public async Task KhoaHoc_GetAll_ContainsExpectedFields()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/khoa-hoc");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var first = doc.RootElement[0];
        Assert.True(first.TryGetProperty("id", out _), "Missing 'id' field");
        Assert.True(first.TryGetProperty("maKhoaHoc", out _), "Missing 'maKhoaHoc' field");
        Assert.True(first.TryGetProperty("tenKhoaHoc", out _), "Missing 'tenKhoaHoc' field");
        Assert.True(first.TryGetProperty("hocPhi", out _), "Missing 'hocPhi' field");
        Assert.True(first.TryGetProperty("trangThai", out _), "Missing 'trangThai' field");
    }

    [Fact]
    public async Task KhoaHoc_GetById_Returns200ForValidId()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/khoa-hoc/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(1, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("KH001", doc.RootElement.GetProperty("maKhoaHoc").GetString());
    }

    [Fact]
    public async Task KhoaHoc_GetById_Returns404ForInvalidId()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/khoa-hoc/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task KhoaHoc_GetAll_FilterByNgonNgu_ReturnsFilteredResults()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/khoa-hoc?ngonNgu=Tiếng+Anh");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        // All results should have ngonNgu = "Tiếng Anh"
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var ngonNgu = item.GetProperty("ngonNgu").GetString();
            Assert.Equal("Tiếng Anh", ngonNgu);
        }
    }

    // --- AI Stats ---

    [Fact]
    public async Task AI_ThongKe_ContainsExpectedFields()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/ai/thong-ke");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("tongKhoaHoc", out _), "Missing 'tongKhoaHoc'");
        Assert.True(doc.RootElement.TryGetProperty("tongHocVien", out _), "Missing 'tongHocVien'");
        Assert.True(doc.RootElement.TryGetProperty("tongGiangVien", out _), "Missing 'tongGiangVien'");
        Assert.True(doc.RootElement.TryGetProperty("tongDangKy", out _), "Missing 'tongDangKy'");
        Assert.True(doc.RootElement.TryGetProperty("doanhThu", out _), "Missing 'doanhThu'");
        Assert.True(doc.RootElement.TryGetProperty("khoaHocPhoBien", out _), "Missing 'khoaHocPhoBien'");
    }

    [Fact]
    public async Task AI_ThongKe_CountsMatchSeededData()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/ai/thong-ke");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Seeded at least 1 KhoaHoc, 1 HocVien, 1 GiangVien
        // (other tests may create additional courses, so use >= 1)
        Assert.True(doc.RootElement.GetProperty("tongKhoaHoc").GetInt32() >= 1,
            "Expected at least 1 khoa hoc");
        Assert.True(doc.RootElement.GetProperty("tongHocVien").GetInt32() >= 1,
            "Expected at least 1 hoc vien");
        Assert.True(doc.RootElement.GetProperty("tongGiangVien").GetInt32() >= 1,
            "Expected at least 1 giang vien");
    }

    [Fact]
    public async Task AI_BaoCao_WithAdminToken_ContainsExpectedFields()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/ai/bao-cao");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("theoNgonNgu", out _), "Missing 'theoNgonNgu'");
        Assert.True(doc.RootElement.TryGetProperty("theoTrinhDo", out _), "Missing 'theoTrinhDo'");
        Assert.True(doc.RootElement.TryGetProperty("doanhThuTheoThang", out _), "Missing 'doanhThuTheoThang'");
        Assert.True(doc.RootElement.TryGetProperty("hocVienMoiTheoThang", out _), "Missing 'hocVienMoiTheoThang'");
    }

    // --- HocVien profile ---

    [Fact]
    public async Task HocVien_CuaToi_ReturnsCorrectProfile()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/hoc-vien/cua-toi");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("HV001", doc.RootElement.GetProperty("maHocVien").GetString());
        Assert.Equal("Học Viên Test", doc.RootElement.GetProperty("hoTen").GetString());
        Assert.Equal("hv@test.vn", doc.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task HocVien_GetAll_WithAdminToken_ReturnsHocVienList()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        var response = await client.GetAsync("/api/hoc-vien");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.GetArrayLength() >= 1, "Expected at least 1 hoc vien");
    }

    // --- DangKy workflow ---

    /// <summary>Creates a fresh MoChuaHoc course as admin and returns its ID.</summary>
    private async Task<int> CreateEnrollableCourseAsync(HttpClient adminClient, string maKhoaHoc)
    {
        var dto = new
        {
            MaKhoaHoc = maKhoaHoc,
            TenKhoaHoc = "Enrollable Course " + maKhoaHoc,
            MoTa = "",
            NgonNgu = "Test",
            TrinhDo = "Test",
            SoBuoiHoc = 5,
            ThoiLuongMoiBuoi = 60,
            HocPhi = 500000m,
            SoLuongToiDa = 30,
            NgayBatDau = DateTime.Now.AddDays(10),
            NgayKetThuc = DateTime.Now.AddDays(60),
            DanhSachLichHoc = new[]
            {
                new { ThuTrongTuan = 1, GioBatDau = "08:00", GioKetThuc = "09:00", PhongHoc = "P001" }
            }
        };

        var resp = await adminClient.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    [Fact]
    public async Task DangKy_HocVienCanEnrollInKhoaHoc()
    {
        // Create a fresh, guaranteed MoChuaHoc course to avoid state contamination
        var adminClient = CreateClient();
        var adminToken = await adminClient.LoginAsync("admin@test.vn", "Admin@123");
        adminClient.SetBearerToken(adminToken!);
        var khoaHocId = await CreateEnrollableCourseAsync(adminClient, "KH_ENROLL1");

        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var payload = new StringContent(
            JsonSerializer.Serialize(new { KhoaHocId = khoaHocId, GhiChu = "Test dang ky" }),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/hoc-vien/dang-ky-khoa-hoc", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("thành công", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DangKy_HocVienCannotEnrollTwice()
    {
        // Create a fresh course for this test
        var adminClient = CreateClient();
        var adminToken = await adminClient.LoginAsync("admin@test.vn", "Admin@123");
        adminClient.SetBearerToken(adminToken!);
        var khoaHocId = await CreateEnrollableCourseAsync(adminClient, "KH_TWICE");

        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        var makePayload = () => new StringContent(
            JsonSerializer.Serialize(new { KhoaHocId = khoaHocId, GhiChu = "Test" }),
            Encoding.UTF8,
            "application/json");

        // First enrollment
        var firstResp = await client.PostAsync("/api/hoc-vien/dang-ky-khoa-hoc", makePayload());
        Assert.Equal(HttpStatusCode.OK, firstResp.StatusCode);

        // Second enrollment (should fail)
        var response = await client.PostAsync("/api/hoc-vien/dang-ky-khoa-hoc", makePayload());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DangKy_CuaToi_AfterEnrollment_ShowsEnrollment()
    {
        // Create a fresh course for this test
        var adminClient = CreateClient();
        var adminToken = await adminClient.LoginAsync("admin@test.vn", "Admin@123");
        adminClient.SetBearerToken(adminToken!);
        var khoaHocId = await CreateEnrollableCourseAsync(adminClient, "KH_HISTORY");

        var client = CreateClient();
        var token = await client.LoginAsync("hv@test.vn", "User@123");
        client.SetBearerToken(token!);

        // Get current enrollment count
        var beforeResp = await client.GetAsync("/api/dang-ky/cua-toi");
        var beforeJson = await beforeResp.Content.ReadAsStringAsync();
        using var beforeDoc = JsonDocument.Parse(beforeJson);
        var countBefore = beforeDoc.RootElement.GetArrayLength();

        // Enroll
        var payload = new StringContent(
            JsonSerializer.Serialize(new { KhoaHocId = khoaHocId, GhiChu = "" }),
            Encoding.UTF8,
            "application/json");
        var enrollResp = await client.PostAsync("/api/hoc-vien/dang-ky-khoa-hoc", payload);
        Assert.Equal(HttpStatusCode.OK, enrollResp.StatusCode);

        // Check history - should have 1 more enrollment
        var response = await client.GetAsync("/api/dang-ky/cua-toi");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.GetArrayLength() > countBefore,
            $"Expected more than {countBefore} enrollments after registering");
    }

    // --- Admin CRUD ---

    [Fact]
    public async Task Admin_CanCreateAndUpdateKhoaHoc()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        // Create
        var createDto = new
        {
            MaKhoaHoc = "KH_CRUD",
            TenKhoaHoc = "CRUD Test Course",
            MoTa = "Test",
            NgonNgu = "Tiếng Nhật",
            TrinhDo = "Trung cấp",
            SoBuoiHoc = 15,
            ThoiLuongMoiBuoi = 90,
            HocPhi = 3000000m,
            SoLuongToiDa = 25,
            NgayBatDau = DateTime.Now.AddDays(30),
            NgayKetThuc = DateTime.Now.AddDays(120),
            DanhSachLichHoc = new[]
            {
                new { ThuTrongTuan = 2, GioBatDau = "09:00", GioKetThuc = "10:30", PhongHoc = "P201" }
            }
        };

        var createResp = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(created);
        var id = doc.RootElement.GetProperty("id").GetInt32();
        Assert.True(id > 0);

        // Update
        var updateDto = new
        {
            MaKhoaHoc = "KH_CRUD",
            TenKhoaHoc = "CRUD Test Course Updated",
            MoTa = "Updated",
            NgonNgu = "Tiếng Nhật",
            TrinhDo = "Nâng cao",
            SoBuoiHoc = 20,
            ThoiLuongMoiBuoi = 90,
            HocPhi = 4000000m,
            SoLuongToiDa = 30,
            NgayBatDau = DateTime.Now.AddDays(30),
            NgayKetThuc = DateTime.Now.AddDays(150),
            DanhSachLichHoc = new[]
            {
                new { ThuTrongTuan = 3, GioBatDau = "10:00", GioKetThuc = "11:30", PhongHoc = "P202" }
            }
        };

        var updateResp = await client.PutAsync($"/api/khoa-hoc/{id}",
            new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        var updated = await updateResp.Content.ReadAsStringAsync();
        using var doc2 = JsonDocument.Parse(updated);
        Assert.Equal("CRUD Test Course Updated", doc2.RootElement.GetProperty("tenKhoaHoc").GetString());
    }

    [Fact]
    public async Task Admin_CapNhatTrangThai_KhoaHoc()
    {
        var client = CreateClient();
        var token = await client.LoginAsync("admin@test.vn", "Admin@123");
        client.SetBearerToken(token!);

        // Create a fresh course specifically for status change test (avoid state pollution)
        var dto = new
        {
            MaKhoaHoc = "KH_STATUS",
            TenKhoaHoc = "Status Test Course",
            MoTa = "",
            NgonNgu = "Tiếng Anh",
            TrinhDo = "Cơ bản",
            SoBuoiHoc = 5,
            ThoiLuongMoiBuoi = 60,
            HocPhi = 1000000m,
            SoLuongToiDa = 20,
            NgayBatDau = DateTime.Now.AddDays(30),
            NgayKetThuc = DateTime.Now.AddDays(90),
            DanhSachLichHoc = new[]
            {
                new { ThuTrongTuan = 1, GioBatDau = "08:00", GioKetThuc = "09:00", PhongHoc = "P001" }
            }
        };
        var createResp = await client.PostAsync("/api/khoa-hoc",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(created);
        var id = createDoc.RootElement.GetProperty("id").GetInt32();

        // Update status to DangHoc (2)
        var response = await client.PatchAsync($"/api/khoa-hoc/{id}/trang-thai",
            new StringContent("2", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify
        var getResp = await client.GetAsync($"/api/khoa-hoc/{id}");
        var json = await getResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("DangHoc", doc.RootElement.GetProperty("trangThai").GetString());
    }
}
