using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanXeMay.Models
{
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }
        public int? MaTaiKhoan { get; set; }
        public TaiKhoan? TaiKhoan { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaBan { get; set; }
        public ICollection<ChiTietGioHang>? ChiTietGioHangs { get; set; }
    }
}