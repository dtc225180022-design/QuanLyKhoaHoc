using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc.Models;

namespace QuanLyKhoaHoc.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<HocVien> HocViens { get; set; }
    public DbSet<GiangVien> GiangViens { get; set; }
    public DbSet<KhoaHoc> KhoaHocs { get; set; }
    public DbSet<LichHoc> LichHocs { get; set; }
    public DbSet<DangKy> DangKys { get; set; }
    public DbSet<PhanCong> PhanCongs { get; set; }
    public DbSet<Diem> Diems { get; set; }
    public DbSet<BuoiHoc> BuoiHocs { get; set; }
    public DbSet<DiemDanh> DiemDanhs { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<BaiViet> BaiViets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NguoiDung>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<HocVien>(e =>
        {
            e.HasIndex(x => x.MaHocVien).IsUnique();
            e.HasOne(x => x.NguoiDung)
             .WithOne(x => x.HocVien)
             .HasForeignKey<HocVien>(x => x.NguoiDungId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GiangVien>(e =>
        {
            e.HasIndex(x => x.MaGiangVien).IsUnique();
            e.HasOne(x => x.NguoiDung)
             .WithOne(x => x.GiangVien)
             .HasForeignKey<GiangVien>(x => x.NguoiDungId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KhoaHoc>(e =>
        {
            e.HasIndex(x => x.MaKhoaHoc).IsUnique();
        });

        modelBuilder.Entity<DangKy>(e =>
        {
            e.HasIndex(x => new { x.HocVienId, x.KhoaHocId }).IsUnique();
            e.HasOne(x => x.HocVien).WithMany(x => x.DanhSachDangKy)
             .HasForeignKey(x => x.HocVienId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.KhoaHoc).WithMany(x => x.DanhSachDangKy)
             .HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Diem>(e =>
        {
            e.HasIndex(x => x.DangKyId).IsUnique();
            e.HasOne(x => x.DangKy).WithOne(x => x.Diem)
             .HasForeignKey<Diem>(x => x.DangKyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PhanCong>(e =>
        {
            e.HasIndex(x => new { x.GiangVienId, x.KhoaHocId }).IsUnique();
            e.HasOne(x => x.GiangVien).WithMany(x => x.DanhSachPhanCong)
             .HasForeignKey(x => x.GiangVienId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.KhoaHoc).WithMany(x => x.DanhSachPhanCong)
             .HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BuoiHoc>(e =>
        {
            e.HasOne(x => x.KhoaHoc).WithMany(x => x.DanhSachBuoiHoc)
             .HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LichHoc).WithMany(x => x.DanhSachBuoiHoc)
             .HasForeignKey(x => x.LichHocId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DiemDanh>(e =>
        {
            e.HasIndex(x => new { x.BuoiHocId, x.DangKyId }).IsUnique();
            e.HasOne(x => x.BuoiHoc).WithMany(x => x.DanhSachDiemDanh)
             .HasForeignKey(x => x.BuoiHocId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.DangKy).WithMany()
             .HasForeignKey(x => x.DangKyId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
