using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBanXeMay.Data;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace WebsiteBanXeMay.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaiKhoanController> _logger;

        public TaiKhoanController(AppDbContext context, ILogger<TaiKhoanController> logger)
        {
            _context = context;
            _logger = logger;
        }

      
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password.Trim());  
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }
        
      
        // ----------------- ĐĂNG KÝ -----------------
        [HttpGet]
        public IActionResult DangKi()
        {
            _logger.LogInformation("👤 GET /DangKi - Truy cập trang đăng ký");
            return View(new DangKiViewModels());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKi(DangKiViewModels model)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("📝 POST /DangKi - Bắt đầu xử lý đăng ký: {TenDangNhap}", model.TenDangNhap);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState invalid - {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return View(model);
                }

                // Trim dữ liệu
                model.TenDangNhap = model.TenDangNhap.Trim();
                model.SoDienThoai = model.SoDienThoai.Trim();
                model.HoTen = model.HoTen.Trim();
                model.DiaChi = model.DiaChi?.Trim();

                _logger.LogInformation("🔍 Kiểm tra trùng lặp - TenDangNhap: {TenDangNhap}, SoDienThoai: {SoDienThoai}",
                    model.TenDangNhap, model.SoDienThoai);

                // Kiểm tra trùng
                var userNameExists = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap);
                if (userNameExists)
                {
                    _logger.LogWarning("⚠️ Tên đăng nhập đã tồn tại: {TenDangNhap}", model.TenDangNhap);
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại!");
                    stopwatch.Stop();
                    _logger.LogInformation("⏱️ Đăng ký thất bại - Thời gian: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                    return View(model);
                }

                var phoneExists = await _context.TaiKhoans.AnyAsync(t => t.SoDienThoai == model.SoDienThoai);
                if (phoneExists)
                {
                    _logger.LogWarning("⚠️ Số điện thoại đã đăng ký: {SoDienThoai}", model.SoDienThoai);
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại đã đăng ký!");
                    stopwatch.Stop();
                    _logger.LogInformation("⏱️ Đăng ký thất bại - Thời gian: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                    return View(model);
                }

                // ✅ Tạo tài khoản
                var hashedPassword = HashPassword(model.MatKhau);
                _logger.LogInformation("🔐 Hash password: {HashedPassword}", hashedPassword[..16] + "...");

                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = hashedPassword,
                    HoTen = model.HoTen,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    TrangThai = true,
                    MaChucVu = 3  // Khách hàng
                };

                _context.TaiKhoans.Add(taiKhoan);
                var rows = await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ Đăng ký THÀNH CÔNG - MaTaiKhoan: {MaTaiKhoan}, Thời gian: {ElapsedMs}ms",
                    taiKhoan.MaTaiKhoan, stopwatch.ElapsedMilliseconds);

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 LỖI đăng ký - User: {TenDangNhap}, Thời gian: {ElapsedMs}ms",
                    model.TenDangNhap, stopwatch.ElapsedMilliseconds);
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại!");
                return View(model);
            }
        }

        // ----------------- ĐĂNG NHẬP -----------------
        [HttpGet]
        public IActionResult DangNhap()
        {
            _logger.LogInformation("🔑 GET /DangNhap - Truy cập trang đăng nhập");
            return View(new DangNhapViewModels());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModels model)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("🔐 POST /DangNhap - Đăng nhập: {TenDangNhap}", model.TenDangNhap);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState invalid đăng nhập");
                    stopwatch.Stop();
                    return View(model);
                }

                var tenDangNhap = model.TenDangNhap.Trim();
                var hashedMatKhau = HashPassword(model.MatKhau);

                _logger.LogInformation("🔍 Tìm user - TenDangNhap: {TenDangNhap}, HashedPass: {HashedPass}",
                    tenDangNhap, hashedMatKhau[..16] + "...");

                var user = await _context.TaiKhoans
                    .Include(t => t.ChucVu)
                    .FirstOrDefaultAsync(t =>
                        t.TenDangNhap == tenDangNhap &&
                        t.MatKhau == hashedMatKhau &&
                        t.TrangThai == true);

                if (user == null)
                {
                    _logger.LogWarning("❌ Đăng nhập thất bại - Không tìm thấy user hợp lệ: {TenDangNhap}", tenDangNhap);
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                    stopwatch.Stop();
                    return View(model);
                }

                // ✅ Lưu session - ChucVu sẽ load đúng
                HttpContext.Session.SetInt32("MaTaiKhoan", user.MaTaiKhoan);
                HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap);
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
                HttpContext.Session.SetInt32("MaChucVu", user.MaChucVu);
                HttpContext.Session.SetString("ChucVu", user.ChucVu?.TenChucVu ?? "Khách hàng");

                stopwatch.Stop();
                _logger.LogInformation("✅ ĐĂNG NHẬP THÀNH CÔNG - {TenDangNhap} ({ChucVu}) - Thời gian: {ElapsedMs}ms",
                    user.TenDangNhap, user.ChucVu.TenChucVu, stopwatch.ElapsedMilliseconds);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 LỖI đăng nhập - User: {TenDangNhap}, Thời gian: {ElapsedMs}ms",
                    model.TenDangNhap, stopwatch.ElapsedMilliseconds);
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại!");
                return View(model);
            }
        }

        // ----------------- ĐĂNG XUẤT -----------------
        public IActionResult DangXuat()
        {
            var userName = HttpContext.Session.GetString("TenDangNhap");
            _logger.LogInformation("🚪 ĐĂNG XUẤT - User: {TenDangNhap}", userName ?? "Unknown");

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ✅ API kiểm tra session
        [HttpGet]
        public IActionResult GetCurrentUser()
        {
            var user = new
            {
                MaTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan"),
                TenDangNhap = HttpContext.Session.GetString("TenDangNhap"),
                HoTen = HttpContext.Session.GetString("HoTen"),
                MaChucVu = HttpContext.Session.GetInt32("MaChucVu"),
                ChucVu = HttpContext.Session.GetString("ChucVu")
            };

            _logger.LogInformation("👀 GetCurrentUser - {UserInfo}",
                user.TenDangNhap ?? "Guest");

            return Json(user);
        }
        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                int soLuong = HttpContext.Session.GetInt32("SoLuongGioHang") ?? 0;
                _logger.LogDebug("🛒 GetCartCount - Số lượng: {SoLuong}", soLuong);
                return Json(soLuong);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Lỗi GetCartCount");
                return Json(0);
            }
        }
        // 🔥 THÊM VÀO CUỐI TaiKhoanController (trước SetUserInfo nếu có)

        // ----------------- ĐỔI MẬT KHẨU -----------------
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            _logger.LogInformation("🔑 GET /DoiMatKhau - Truy cập trang đổi mật khẩu");

            // Kiểm tra đăng nhập
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null || maTaiKhoan == 0)
            {
                TempData["error"] = "❌ Vui lòng đăng nhập để đổi mật khẩu!";
                return RedirectToAction("DangNhap");
            }

            return View(new DoiMatKhauViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("🔐 POST /DoiMatKhau - Bắt đầu đổi mật khẩu");

            try
            {
                var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

                if (maTaiKhoan == null || maTaiKhoan == 0)
                {
                    _logger.LogWarning("❌ Không tìm thấy session MaTaiKhoan");
                    TempData["error"] = "❌ Phiên đăng nhập hết hạn!";
                    return RedirectToAction("DangNhap");
                }

                // ✅ VALIDATE MODEL
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState invalid đổi mật khẩu");
                    return View(model);
                }

                // Trim dữ liệu
                model.MatKhauCu = model.MatKhauCu?.Trim();
                model.MatKhauMoi = model.MatKhauMoi?.Trim();
                model.XacNhanMatKhau = model.XacNhanMatKhau?.Trim();

                // 🔥 KIỂM TRA MẬT KHẨU CŨ
                var hashedMatKhauCu = HashPassword(model.MatKhauCu);
                var user = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan &&
                                            t.MatKhau == hashedMatKhauCu &&
                                            t.TrangThai == true);

                if (user == null)
                {
                    _logger.LogWarning("❌ Mật khẩu cũ sai - MaTaiKhoan: {MaTaiKhoan}", maTaiKhoan);
                    ModelState.AddModelError("MatKhauCu", "❌ Mật khẩu cũ không đúng!");
                    return View(model);
                }

                // 🔥 KIỂM TRA MẬT KHẨU MỚI KHÔNG GIỐNG CŨ
                if (hashedMatKhauCu == HashPassword(model.MatKhauMoi))
                {
                    _logger.LogWarning("⚠️ Mật khẩu mới giống mật khẩu cũ - MaTaiKhoan: {MaTaiKhoan}", maTaiKhoan);
                    ModelState.AddModelError("MatKhauMoi", "❌ Mật khẩu mới không được giống mật khẩu cũ!");
                    return View(model);
                }

                // 🔥 KIỂM TRA XÁC NHẬN MẬT KHẨU
                if (model.MatKhauMoi != model.XacNhanMatKhau)
                {
                    _logger.LogWarning("❌ Xác nhận mật khẩu không khớp");
                    ModelState.AddModelError("XacNhanMatKhau", "❌ Mật khẩu xác nhận không khớp!");
                    return View(model);
                }

                // ✅ CẬP NHẬT MẬT KHẨU MỚI
                var hashedMatKhauMoi = HashPassword(model.MatKhauMoi);
                user.MatKhau = hashedMatKhauMoi;

                var rows = await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ ĐỔI MẬT KHẨU THÀNH CÔNG - MaTaiKhoan: {MaTaiKhoan}, Thời gian: {ElapsedMs}ms",
                    maTaiKhoan, stopwatch.ElapsedMilliseconds);

                // 🔥 THÔNG BÁO + TRỞ VỀ TRANG ĐỔI MẬT KHẨU (KHÔNG REDIRECT)
                TempData["success"] = "Đổi mật khẩu thành công!";

                // 🔥 QUAN TRỌNG: Trở về chính trang DoiMatKhau để JS chạy
                return RedirectToAction("DoiMatKhau");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 LỖI đổi mật khẩu - MaTaiKhoan: {MaTaiKhoan}, Thời gian: {ElapsedMs}ms",
                    HttpContext.Session.GetInt32("MaTaiKhoan"), stopwatch.ElapsedMilliseconds);
                ModelState.AddModelError("", "❌ Có lỗi xảy ra, vui lòng thử lại!");
                return View(model);
            }
        }
        // 🔥 GIỎ HÀNG 
        [HttpGet]
        public async Task<IActionResult> GioHang()
        {
            _logger.LogInformation("🛒 GioHang - UserID: {MaTaiKhoan}", HttpContext.Session.GetInt32("MaTaiKhoan"));

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null || maTaiKhoan == 0)
            {
                _logger.LogWarning("❌ Chưa đăng nhập");
                TempData["Loi"] = "Vui lòng đăng nhập để xem giỏ hàng!";
                return RedirectToAction("DangNhap");
            }

            try
            {
                
                var chiTietGioHang = await _context.ChiTietGioHangs
                    .Include(ct => ct.SanPham)
                    .Include(ct => ct.GioHang)  
                    .Where(ct => ct.GioHang.MaTaiKhoan == maTaiKhoan.Value)
                    .ToListAsync();

                ViewBag.TongTien = chiTietGioHang.Sum(ct => ct.SoLuong * ct.SanPham.Gia);
                ViewBag.SoLuongSanPham = chiTietGioHang.Sum(ct => ct.SoLuong);
                ViewBag.DebugUserId = maTaiKhoan;
                ViewBag.DebugCount = chiTietGioHang.Count;

                _logger.LogInformation("✅ GioHang OK - User: {0}, SP: {1}", maTaiKhoan, chiTietGioHang.Count);
                return View("~/Views/GioHang/Index.cshtml", chiTietGioHang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Lỗi GioHang");
                return View(new List<ChiTietGioHang>());
            }
        }
    }
}