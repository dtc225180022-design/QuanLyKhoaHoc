using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── PORT (Railway sets $PORT automatically) ───────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// ─── DATABASE ───────────────────────────────────────────────────────
// Railway cung cấp DATABASE_URL (PostgreSQL). Dev dùng SQL Server.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway PostgreSQL: postgres://user:pass@host:port/db
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(databaseUrl));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAIRecommendService, AIRecommendService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "QuanLyKhoaHoc_SuperSecretKey_2024_TrungTamNgoaiNgu_Jwt";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuanLyKhoaHoc",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "QuanLyKhoaHoc",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Đọc token từ cookie để MVC views hoạt động
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.ContainsKey("jwt_token"))
                ctx.Token = ctx.Request.Cookies["jwt_token"];
            return Task.CompletedTask;
        },
        // Redirect về trang đăng nhập khi MVC views bị unauthorized
        OnChallenge = ctx =>
        {
            if (!ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.HandleResponse();
                ctx.Response.Redirect("/Auth/DangNhap");
            }
            return Task.CompletedTask;
        },
        OnForbidden = ctx =>
        {
            if (!ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.Redirect("/Error/403");
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Controllers + Views + API
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        // Đảm bảo JSON trả về camelCase để JavaScript đọc được
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quản Lý Khóa Học API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
    {
        // PostgreSQL (Railway): dùng EnsureCreated vì migrations viết cho SQL Server
        // SQL Server (dev): dùng Migrate() để apply đúng migration
        var isPostgres = db.Database.ProviderName?.Contains("Npgsql") == true;
        if (isPostgres)
            db.Database.EnsureCreated();
        else
            db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated(); // InMemory (tests)
    }
    SeedData.Initialize(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Tắt HTTPS redirect trên Railway (Railway tự xử lý TLS ở proxy layer)
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quản Lý Khóa Học API v1"));
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Short URL: /TinTuc/{id} → TinTucController.ChiTiet(id)
app.MapControllerRoute(
    name: "tintuc-chitiet",
    pattern: "TinTuc/{id:int}",
    defaults: new { controller = "TinTuc", action = "ChiTiet" });

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// Friendly URL routes
app.MapGet("/dang-nhap", ctx => { ctx.Response.Redirect("/Auth/DangNhap"); return Task.CompletedTask; });
app.MapGet("/dang-ky", ctx => { ctx.Response.Redirect("/Auth/DangKy"); return Task.CompletedTask; });
app.MapGet("/admin", ctx => { ctx.Response.Redirect("/Admin/Index"); return Task.CompletedTask; });
app.MapGet("/khoa-hoc", ctx => { ctx.Response.Redirect("/KhoaHoc"); return Task.CompletedTask; });
app.MapGet("/giang-vien", ctx => { ctx.Response.Redirect("/GiangVien"); return Task.CompletedTask; });
app.MapGet("/diem-cua-toi", ctx => { ctx.Response.Redirect("/HocVien/KetQua"); return Task.CompletedTask; });
app.MapGet("/ho-so", ctx => { ctx.Response.Redirect("/HocVien/HoSo"); return Task.CompletedTask; });
app.MapGet("/ho-so-giang-vien", ctx => { ctx.Response.Redirect("/GiangVien/HoSo"); return Task.CompletedTask; });
app.MapGet("/khoa-hoc-cua-toi", ctx => { ctx.Response.Redirect("/HocVien/LichSuDangKy"); return Task.CompletedTask; });
app.MapGet("/thanh-toan/{id:int}", ctx => { ctx.Response.Redirect($"/HocVien/ThanhToan/{ctx.Request.RouteValues["id"]}"); return Task.CompletedTask; });

app.MapControllers();

app.Run();

// Expose Program class to test project
public partial class Program { }
