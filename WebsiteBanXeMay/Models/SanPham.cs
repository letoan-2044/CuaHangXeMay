using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models
{
    public class SanPham
    {
        [Key]
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; } = "";
        public decimal Gia { get; set; }
        public int SoLuongTon { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public int? MaDanhMuc { get; set; }  // ✅ Match DB
        public bool TrangThai { get; set; } = true;

        public DanhMuc? DanhMuc { get; set; }
    }
}