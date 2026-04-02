using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 🔥 EXISTING TABLES
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<ChucVu> ChucVus { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

        // 🔥 NEW TABLES - DONHANG
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔥 TABLE NAMES
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<ChucVu>().ToTable("ChucVu");
            modelBuilder.Entity<DanhMuc>().ToTable("DanhMuc");
            modelBuilder.Entity<SanPham>().ToTable("SanPham");
            modelBuilder.Entity<GioHang>().ToTable("GioHang");
            modelBuilder.Entity<ChiTietGioHang>().ToTable("ChiTietGioHang");

            // 🔥 NEW TABLE NAMES
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHang");

            // 🔥 FK SanPham -> DanhMuc
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.DanhMuc)
                .WithMany()
                .HasForeignKey(s => s.MaDanhMuc)
                .HasPrincipalKey(d => d.MaDanhMuc)
                .HasConstraintName("FK_SanPham_DanhMuc")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // 🔥 FK TaiKhoan -> ChucVu
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.ChucVu)
                .WithMany()
                .HasForeignKey(t => t.MaChucVu)
                .HasPrincipalKey(c => c.MaChucVu)
                .HasConstraintName("FK_TaiKhoan_ChucVu")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // 🔥 GIOHANG RELATIONSHIPS
            modelBuilder.Entity<GioHang>()
                .HasOne(g => g.TaiKhoan)
                .WithMany()
                .HasForeignKey(g => g.MaTaiKhoan)
                .HasPrincipalKey(t => t.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.GioHang)
                .WithMany(g => g.ChiTietGioHangs)
                .HasForeignKey(ct => ct.MaGioHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(ct => ct.SanPham)
                .WithMany()
                .HasForeignKey(ct => ct.MaSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔥 🔥 DONHANG RELATIONSHIPS - MỚI ✅
            modelBuilder.Entity<DonHang>()
                .HasOne(dh => dh.TaiKhoan)
                .WithMany()
                .HasForeignKey(dh => dh.MaTaiKhoan)
                .HasPrincipalKey(t => t.MaTaiKhoan)
                .HasConstraintName("FK_DonHang_TaiKhoan")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(dh => dh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaDonHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.SanPham)
                .WithMany()
                .HasForeignKey(ct => ct.MaSanPham)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔥 COLUMN TYPES & INDEXES
            modelBuilder.Entity<SanPham>()
                .Property(s => s.Gia)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DonHang>()
                .Property(dh => dh.TongTien)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(ct => ct.GiaBan)
                .HasColumnType("decimal(18,2)");

            // 🔥 INDEXES FOR PERFORMANCE
            modelBuilder.Entity<SanPham>().HasIndex(s => s.TrangThai);
            modelBuilder.Entity<DonHang>().HasIndex(dh => dh.MaTaiKhoan);
            modelBuilder.Entity<DonHang>().HasIndex(dh => dh.NgayDat);
            modelBuilder.Entity<DonHang>().HasIndex(dh => dh.TrangThai);

            // 🔥 SEED DATA - EXISTING
            modelBuilder.Entity<ChucVu>().HasData(
                new ChucVu { MaChucVu = 1, TenChucVu = "Admin" },
                new ChucVu { MaChucVu = 2, TenChucVu = "Nhân viên" },
                new ChucVu { MaChucVu = 3, TenChucVu = "Khách hàng" }
            );

            modelBuilder.Entity<DanhMuc>().HasData(
                new DanhMuc { MaDanhMuc = 1, TenDanhMuc = "Xe số" },
                new DanhMuc { MaDanhMuc = 2, TenDanhMuc = "Xe ga" },
                new DanhMuc { MaDanhMuc = 3, TenDanhMuc = "Xe thể thao" },
                new DanhMuc { MaDanhMuc = 4, TenDanhMuc = "Xe điện" }
            );

            modelBuilder.Entity<SanPham>().HasData(
                new SanPham
                {
                    MaSanPham = 1,
                    TenSanPham = "Honda Wave Alpha 110",
                    Gia = 17900000m,
                    SoLuongTon = 50,
                    MoTa = "Xe số phổ thông tiết kiệm nhiên liệu",
                    HinhAnh = "wave-alpha.png",
                    MaDanhMuc = 1,
                    TrangThai = true
                },
                new SanPham
                {
                    MaSanPham = 2,
                    TenSanPham = "Honda Air Blade 125",
                    Gia = 32900000m,
                    SoLuongTon = 30,
                    MoTa = "Xe ga thời trang, vận hành êm ái",
                    HinhAnh = "airblade.png",
                    MaDanhMuc = 2,
                    TrangThai = true
                },
                new SanPham
                {
                    MaSanPham = 3,
                    TenSanPham = "Yamaha Exciter 155",
                    Gia = 48900000m,
                    SoLuongTon = 20,
                    MoTa = "Xe côn tay thể thao mạnh mẽ",
                    HinhAnh = "exciter.png",
                    MaDanhMuc = 1,
                    TrangThai = true
                }
            );

            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    MaTaiKhoan = 1,
                    TenDangNhap = "admin",
                    MatKhau = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", // 123456
                    HoTen = "Quản Trị Viên",
                    SoDienThoai = "0901234567",
                    DiaChi = "An Giang",
                    MaChucVu = 1,
                    TrangThai = true
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 2,
                    TenDangNhap = "nhanvien",
                    MatKhau = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", // 123456
                    HoTen = "Nhân Viên Bán Hàng",
                    SoDienThoai = "0987654321",
                    DiaChi = "An Giang",
                    MaChucVu = 2,
                    TrangThai = true
                },
                new TaiKhoan
                {
                    MaTaiKhoan = 3,
                    TenDangNhap = "khachhang",
                    MatKhau = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", // 123456
                    HoTen = "Khách Hàng Test",
                    SoDienThoai = "0912345678",
                    DiaChi = "An Giang",
                    MaChucVu = 3,
                    TrangThai = true
                }
            );
        }
    }
}