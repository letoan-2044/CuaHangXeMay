using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models
{
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }
        public int? MaTaiKhoan { get; set; }

        // Navigation
        public TaiKhoan? TaiKhoan { get; set; }
        public ICollection<ChiTietGioHang>? ChiTietGioHangs { get; set; }
    }
}