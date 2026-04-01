using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models
{
    public class ChiTietGioHang
    {
        [Key]
        public int MaChiTietGio { get; set; }
        public int MaGioHang { get; set; }
        public int MaSanPham { get; set; }
        public int SoLuong { get; set; }

        // Navigation
        public GioHang? GioHang { get; set; }
        public SanPham? SanPham { get; set; }
    }
}