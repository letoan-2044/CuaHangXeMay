using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models.ViewModels
{
    public class DangKiViewModels
    {
        [Required(ErrorMessage = "Tên đăng nhập bắt buộc"), MinLength(3, ErrorMessage = "Tối thiểu 3 ký tự")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Số điện thoại bắt buộc"), Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Họ tên bắt buộc"), MaxLength(100)]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu bắt buộc"), MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Xác nhận mật khẩu bắt buộc"), Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; } = "";

        public string? DiaChi { get; set; }
    }
}