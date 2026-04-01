using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models.ViewModels
{
    public class DangNhapViewModels
    {
        [Required(ErrorMessage = "Tên đăng nhập bắt buộc")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu bắt buộc")]
        public string MatKhau { get; set; } = "";
    }
}