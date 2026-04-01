using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanXeMay.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int? MaDonHang { get; set; }
        public int? MaSanPham { get; set; }

        public int SoLuong { get; set; }

        public decimal GiaBan { get; set; }

        [ForeignKey("MaDonHang")]
        public DonHang? DonHang { get; set; }

        [ForeignKey("MaSanPham")]
        public SanPham? SanPham { get; set; }
    }
}