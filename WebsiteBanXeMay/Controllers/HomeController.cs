using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data; // AppDbContext namespace
using System.Collections.Generic;

namespace WebsiteBanXeMay.Controllers
{
    public class HomeController : Controller  // ✅ Kế thừa Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // ✅ Constructor Dependency Injection
        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ TRANG CHỦ
        public async Task<IActionResult> Index()
        {
            var userName = HttpContext.Session.GetString("TenDangNhap") ?? "Khách";
            ViewBag.UserName = userName;

            var sanPhams = await _context.SanPhams
                .Where(s => s.TrangThai == true)
                .Take(8)
                .ToListAsync();

            return View(sanPhams);
        }

        // ✅ TEST DATABASE - QUAN TRỌNG!
        public async Task<IActionResult> TestDb()
        {
            var result = new Dictionary<string, string>();

            try
            {
                _logger.LogInformation("🔍 Test database...");

                // 1. Connection
                var canConnect = await _context.Database.CanConnectAsync();
                result["Connection"] = canConnect ? "🟢 KẾT NỐI OK" : "🔴 LỖI";

                // 2. Số tài khoản
                var countTaiKhoan = await _context.TaiKhoans.CountAsync();
                result["TaiKhoan"] = $"📊 {countTaiKhoan} tài khoản";

                // 3. Admin
                var admin = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == "admin");
                result["Admin"] = admin != null
                    ? $"👑 {admin.HoTen} (Pass: {admin.MatKhau})"
                    : "❌ KHÔNG CÓ";

                // 4. Session
                var sessionUser = HttpContext.Session.GetString("TenDangNhap");
                result["Session"] = sessionUser ?? "👤 Khách";

                ViewBag.Status = "🎉 TẤT CẢ OK!";
                _logger.LogInformation("✅ Test PASS");
            }
            catch (Exception ex)
            {
                result["🔴 LỖI"] = ex.Message;
                ViewBag.Status = "💥 CÓ LỖI!";
                _logger.LogError(ex, "Test FAIL");
            }

            ViewBag.TestResult = result;
            return View();
        }

        // ERROR
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}