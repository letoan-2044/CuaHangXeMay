using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanXeMay.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        public int? MaTaiKhoan { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal? TongTien { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? TrangThai { get; set; } = "Chờ xử lý";

        // Navigation
        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan? TaiKhoan { get; set; }
        public ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
    }
}