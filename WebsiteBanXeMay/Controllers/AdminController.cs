using Microsoft.AspNetCore.Mvc;
using WebsiteBanXeMay.Data;

namespace WebsiteBanXeMay.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ✅ CHECK QUYỀN SESSION
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền truy cập Admin!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            ViewBag.MaChucVu = maChucVu;
            ViewBag.TenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            ViewBag.TongSanPham = _context.SanPhams.Count();

            return View();
        }

        public IActionResult SanPham()
        {
            return RedirectToAction("Index", "AdminSanPham");
        }
    }
}