using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanXeMay.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required, MaxLength(50), MinLength(3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage = "Tên đăng nhập chỉ chứa chữ, số, dấu _")]
        public string TenDangNhap { get; set; } = "";

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [MaxLength(100)]
        public string? HoTen { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số ĐT: 10 số, bắt đầu bằng 0")]
        [MaxLength(15)]
        public string? SoDienThoai { get; set; }

        [MaxLength(200)]
        public string? DiaChi { get; set; }

        // ✅ 🔥 SỬA CHÍNH: Bỏ ? - Không nullable
        [Required]
        public int MaChucVu { get; set; } = 3;

        [Required]
        public bool TrangThai { get; set; } = true;

        [ForeignKey("MaChucVu")]
        public virtual ChucVu? ChucVu { get; set; }
    }
}