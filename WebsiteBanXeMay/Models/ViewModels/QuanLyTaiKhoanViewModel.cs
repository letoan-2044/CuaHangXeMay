// Models/ViewModels/QuanLyTaiKhoanViewModel.cs
namespace WebsiteBanXeMay.Models.ViewModels
{
    public class QuanLyTaiKhoanViewModel
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? Email { get; set; }
        public bool TrangThai { get; set; }
        public string TenChucVu { get; set; } = string.Empty;
        public int MaChucVu { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}