using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models
{
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required, MaxLength(100)]
        public string TenDanhMuc { get; set; } = "";

        public string? MoTa { get; set; }

        // Navigation property
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}