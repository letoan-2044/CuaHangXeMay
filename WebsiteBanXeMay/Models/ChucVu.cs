using System.ComponentModel.DataAnnotations;

namespace WebsiteBanXeMay.Models
{
    public class ChucVu
    {
        [Key]
        public int MaChucVu { get; set; }

        [Required, MaxLength(50)]
        public string TenChucVu { get; set; } = "";
    }
}