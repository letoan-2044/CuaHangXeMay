// Models/ViewModels/DoiMatKhauViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models.ViewModels
{
    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "❌ Vui lòng nhập mật khẩu cũ!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string MatKhauCu { get; set; } = string.Empty;

        [Required(ErrorMessage = "❌ Vui lòng nhập mật khẩu mới!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "❌ Mật khẩu mới phải có ít nhất 6 ký tự!")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "❌ Vui lòng xác nhận mật khẩu mới!")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("MatKhauMoi", ErrorMessage = "❌ Mật khẩu xác nhận không khớp!")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}