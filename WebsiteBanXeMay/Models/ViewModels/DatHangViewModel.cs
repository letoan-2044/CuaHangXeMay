using System.ComponentModel.DataAnnotations;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Models
{
    public class DatHangViewModel
    {
        public List<ChiTietGioHang> ChiTietGioHangs { get; set; } = new();
        public DonHang DonHang { get; set; } = new();
        public decimal TongTien { get; set; }
    }
}