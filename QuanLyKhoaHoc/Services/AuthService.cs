using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyKhoaHoc.Data;
using QuanLyKhoaHoc.DTOs;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<bool> RegisterAsync(RegisterDto dto);
    Task<bool> DoiMatKhauAsync(int userId, DoiMatKhauDto dto);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.NguoiDungs
            .FirstOrDefaultAsync(x => x.Email == dto.Email && x.HoatDong);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.MatKhau, user.MatKhauHash))
            return null;

        var token = TaoToken(user);
        return new LoginResponseDto
        {
            Token = token,
            HoTen = user.HoTen,
            Email = user.Email,
            VaiTro = user.VaiTro.ToString(),
            AnhDaiDien = user.AnhDaiDien,
            HetHan = DateTime.UtcNow.AddHours(8)
        };
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        if (await _db.NguoiDungs.AnyAsync(x => x.Email == dto.Email))
            return false;

        var soThuTu = await _db.HocViens.CountAsync() + 1;
        var user = new NguoiDung
        {
            HoTen = dto.HoTen,
            Email = dto.Email,
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
            SoDienThoai = dto.SoDienThoai,
            VaiTro = VaiTro.User
        };

        _db.NguoiDungs.Add(user);
        await _db.SaveChangesAsync();

        _db.HocViens.Add(new HocVien
        {
            NguoiDungId = user.Id,
            MaHocVien = $"HV{soThuTu:D4}"
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DoiMatKhauAsync(int userId, DoiMatKhauDto dto)
    {
        var user = await _db.NguoiDungs.FindAsync(userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.MatKhauCu, user.MatKhauHash))
            return false;

        user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhauMoi);
        await _db.SaveChangesAsync();
        return true;
    }

    private string TaoToken(NguoiDung user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:SecretKey"] ?? "QuanLyKhoaHoc_SuperSecretKey_2024_TrungTamNgoaiNgu_Jwt"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim(ClaimTypes.Role, user.VaiTro.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "QuanLyKhoaHoc",
            audience: _config["Jwt:Audience"] ?? "QuanLyKhoaHoc",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
