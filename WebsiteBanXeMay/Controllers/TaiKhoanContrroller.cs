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

                // 🔥 BƯỚC 1: TÌM USER THEO TÊN ĐĂNG NHẬP (KHÔNG KIỂM TRA MẬT KHẨU)
                var user = await _context.TaiKhoans
                    .Include(t => t.ChucVu)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

                if (user == null)
                {
                    _logger.LogWarning("❌ Đăng nhập thất bại - Không tìm thấy user: {TenDangNhap}", tenDangNhap);
                    ModelState.AddModelError("", "👤 Tên đăng nhập hoặc mật khẩu không đúng!");
                    stopwatch.Stop();
                    return View(model);
                }

                // 🔥 TRƯỜNG HỢP 1: TÀI KHOẢN BỊ KHÓA
                if (!user.TrangThai)
                {
                    _logger.LogWarning("🔒 Đăng nhập thất bại - Tài khoản bị khóa: {TenDangNhap}", tenDangNhap);
                    ModelState.AddModelError("", "🚫 Tài khoản của bạn đã bị khóa! Vui lòng liên hệ quản trị viên.");
                    stopwatch.Stop();
                    return View(model);
                }

                // 🔥 TRƯỜNG HỢP 2: MẬT KHẨU SAI
                if (user.MatKhau != hashedMatKhau)
                {
                    _logger.LogWarning("❌ Đăng nhập thất bại - Mật khẩu sai: {TenDangNhap}", tenDangNhap);
                    ModelState.AddModelError("", "👤 Tên đăng nhập hoặc mật khẩu không đúng!");
                    stopwatch.Stop();
                    return View(model);
                }

                // ✅ ĐĂNG NHẬP THÀNH CÔNG (code cũ giữ nguyên)

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
        [HttpGet]
        public async Task<IActionResult> QuanLyTaiKhoan(int page = 1, int pageSize = 10, string? search = null, int? trangThai = null)
        {
            _logger.LogInformation("🔍 QuanLyTaiKhoan - Page: {Page}, Search: {Search}, TrangThai: {TrangThai}",
                page, search, trangThai);

            var maChucVu = HttpContext.Session.GetInt32("MaChucVu") ?? 0;
            if (maChucVu > 2)
            {
                TempData["error"] = "❌ Không có quyền truy cập!";
                return RedirectToAction("Index", "Home");
            }

            // 🔥 QUERY DATABASE
            var query = _context.TaiKhoans
                .Include(t => t.ChucVu)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(t =>
                    t.TenDangNhap.ToLower().Contains(s) ||
                    (t.HoTen ?? "").ToLower().Contains(s) ||
                    (t.SoDienThoai ?? "").Contains(s));
            }

            // Lọc trạng thái
            if (trangThai.HasValue)
                query = query.Where(t => t.TrangThai == (trangThai.Value == 1));

            // 🔥 ĐẾM TỔNG
            var totalItems = await query.CountAsync();
            _logger.LogInformation("📊 Total items: {Total}", totalItems);

            // 🔥 LẤY DỮ LIỆU PHÂN TRANG
            var taiKhoans = await query
                .OrderByDescending(t => t.MaTaiKhoan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("✅ Loaded {Count} accounts for page {Page}", taiKhoans.Count, page);

            // 🔥 TRUYỀN DATA VÀO VIEWBAG
            ViewBag.TaiKhoans = taiKhoans;  
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search ?? "";
            ViewBag.TrangThai = trangThai;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int maTaiKhoan)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan);
            if (taiKhoan == null) return Json(new { success = false });

            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTaiKhoan(int maTaiKhoan)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("🗑️ Bắt đầu xóa tài khoản ID: {MaTaiKhoan}", maTaiKhoan);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 🔥 XÓA TẤT CẢ DỮ LIỆU LIÊN QUAN
                #region Xóa ChiTietDonHang
                var chiTietDonHangs = await _context.ChiTietDonHangs
                    .Where(ct => ct.DonHang.MaTaiKhoan == maTaiKhoan)
                    .ToListAsync();
                _context.ChiTietDonHangs.RemoveRange(chiTietDonHangs);
                #endregion

                #region Xóa DonHang
                var donHangs = await _context.DonHangs
                    .Where(d => d.MaTaiKhoan == maTaiKhoan)
                    .ToListAsync();
                _context.DonHangs.RemoveRange(donHangs);
                #endregion

                #region Xóa ChiTietGioHang
                var chiTietGioHangs = await _context.ChiTietGioHangs
                    .Where(ct => ct.GioHang.MaTaiKhoan == maTaiKhoan)
                    .ToListAsync();
                _context.ChiTietGioHangs.RemoveRange(chiTietGioHangs);
                #endregion

                #region Xóa GioHang
                var gioHangs = await _context.GioHangs
                    .Where(g => g.MaTaiKhoan == maTaiKhoan)
                    .ToListAsync();
                _context.GioHangs.RemoveRange(gioHangs);
                #endregion

                #region Xóa TaiKhoan
                var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan);
                if (taiKhoan == null)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
                }
                _context.TaiKhoans.Remove(taiKhoan);
                #endregion

                // 🔥 SAVE ALL
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stopwatch.Stop();
                _logger.LogInformation("✅ XÓA HOÀN TOÀN - ID: {MaTaiKhoan} ({ElapsedMs}ms)",
                    maTaiKhoan, stopwatch.ElapsedMilliseconds);

                return Json(new
                {
                    success = true,
                    message = "Xóa tài khoản và tất cả dữ liệu liên quan thành công!",
                    reload = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 LỖI XÓA TÀI KHOẢN {MaTaiKhoan}", maTaiKhoan);
                return Json(new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ThongTin(int id)
        {
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu") ?? 0;
            if (maChucVu > 2)
            {
                TempData["error"] = "❌ Không có quyền truy cập!";
                return RedirectToAction("Index", "Home");
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.ChucVu)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
            {
                TempData["error"] = "❌ Không tìm thấy tài khoản!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            return View(taiKhoan);
        }
        // ----------------- SỬA THÔNG TIN TÀI KHOẢN -----------------
        [HttpGet]
        public async Task<IActionResult> SuaThongTin(int id)
        {
            _logger.LogInformation("✏️ GET SuaThongTin - ID: {Id}", id);

            var maChucVu = HttpContext.Session.GetInt32("MaChucVu") ?? 0;
            if (maChucVu > 2)
            {
                TempData["error"] = "❌ Không có quyền truy cập!";
                return RedirectToAction("Index", "Home");
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.ChucVu)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
            {
                TempData["error"] = "❌ Không tìm thấy tài khoản!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            var model = new SuaThongTinViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                SoDienThoai = taiKhoan.SoDienThoai,
                DiaChi = taiKhoan.DiaChi,
                Email = taiKhoan.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaThongTin(SuaThongTinViewModel model)
        {
            _logger.LogInformation("✏️ POST SuaThongTin - ID: {Id}", model.MaTaiKhoan);

            var maChucVu = HttpContext.Session.GetInt32("MaChucVu") ?? 0;
            if (maChucVu > 2)
            {
                TempData["error"] = "❌ Không có quyền truy cập!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState invalid - {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return View(model);
                }

                var taiKhoan = await _context.TaiKhoans.FindAsync(model.MaTaiKhoan);
                if (taiKhoan == null)
                {
                    TempData["error"] = "❌ Không tìm thấy tài khoản!";
                    return RedirectToAction("QuanLyTaiKhoan");
                }

                // ✅ Cập nhật thông tin (TenDangNhap KHÔNG đổi được)
                taiKhoan.HoTen = model.HoTen?.Trim();
                taiKhoan.SoDienThoai = model.SoDienThoai?.Trim();
                taiKhoan.DiaChi = model.DiaChi?.Trim();
                taiKhoan.Email = model.Email?.Trim();

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Sửa thành công - ID: {Id}", model.MaTaiKhoan);

                TempData["success"] = "✅ Cập nhật thông tin thành công!";
                return RedirectToAction("ThongTin", new { id = model.MaTaiKhoan });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "💥 Lỗi DB khi sửa - ID: {Id}", model.MaTaiKhoan);
                ModelState.AddModelError("", "❌ Có lỗi cơ sở dữ liệu, vui lòng thử lại!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Lỗi sửa thông tin - ID: {Id}", model.MaTaiKhoan);
                ModelState.AddModelError("", "❌ Có lỗi xảy ra, vui lòng thử lại!");
            }

            return View(model);
        }
        // 🔥 THÔNG TIN CÁ NHÂN - KHÁCH HÀNG
        [HttpGet]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            _logger.LogInformation("👤 ThongTinCaNhan - UserID: {MaTaiKhoan}",
                HttpContext.Session.GetInt32("MaTaiKhoan"));

            // Kiểm tra đăng nhập
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null || maTaiKhoan == 0)
            {
                TempData["error"] = "❌ Vui lòng đăng nhập để xem thông tin cá nhân!";
                return RedirectToAction("DangNhap");
            }

            try
            {
                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.ChucVu)
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

                if (taiKhoan == null)
                {
                    _logger.LogWarning("❌ Không tìm thấy tài khoản trong session: {MaTaiKhoan}", maTaiKhoan);
                    TempData["error"] = "❌ Thông tin tài khoản không hợp lệ!";
                    return RedirectToAction("DangNhap");
                }

                _logger.LogInformation("✅ ThongTinCaNhan OK - User: {TenDangNhap}", taiKhoan.TenDangNhap);
                return View(taiKhoan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Lỗi ThongTinCaNhan - UserID: {MaTaiKhoan}", maTaiKhoan);
                TempData["error"] = "❌ Có lỗi xảy ra, vui lòng thử lại!";
                return RedirectToAction("DangNhap");
            }
        }

        // 🔥 SỬA THÔNG TIN CÁ NHÂN - KHÁCH HÀNG
        [HttpGet]
        public async Task<IActionResult> SuaThongTinCaNhan()
        {
            _logger.LogInformation("✏️ SuaThongTinCaNhan - UserID: {MaTaiKhoan}",
                HttpContext.Session.GetInt32("MaTaiKhoan"));

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null || maTaiKhoan == 0)
            {
                return RedirectToAction("DangNhap");
            }

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

            if (taiKhoan == null) return RedirectToAction("DangNhap");

            var model = new SuaThongTinViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                SoDienThoai = taiKhoan.SoDienThoai,
                DiaChi = taiKhoan.DiaChi,
                Email = taiKhoan.Email
            };

            return View("SuaThongTin", model);  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaThongTinCaNhan(SuaThongTinViewModel model)
        {
            _logger.LogInformation("✏️ POST SuaThongTinCaNhan - ID: {Id}", model.MaTaiKhoan);

            var maTaiKhoanSession = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoanSession != model.MaTaiKhoan)
            {
                TempData["error"] = "❌ Không có quyền chỉnh sửa tài khoản này!";
                return RedirectToAction("ThongTinCaNhan");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState invalid SuaThongTinCaNhan");
                    return View(model);
                }

                var taiKhoan = await _context.TaiKhoans.FindAsync(model.MaTaiKhoan);
                if (taiKhoan == null)
                {
                    TempData["error"] = "❌ Không tìm thấy tài khoản!";
                    return RedirectToAction("ThongTinCaNhan");
                }

                // ✅ Cập nhật (không cho đổi TenDangNhap)
                taiKhoan.HoTen = model.HoTen?.Trim();
                taiKhoan.SoDienThoai = model.SoDienThoai?.Trim();
                taiKhoan.DiaChi = model.DiaChi?.Trim();
                taiKhoan.Email = model.Email?.Trim();

                // Cập nhật session HoTen
                HttpContext.Session.SetString("HoTen", taiKhoan.HoTen ?? "");

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ SuaThongTinCaNhan thành công - ID: {Id}", model.MaTaiKhoan);

                TempData["success"] = "✅ Cập nhật thông tin thành công!";
                return RedirectToAction("ThongTinCaNhan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Lỗi SuaThongTinCaNhan - ID: {Id}", model.MaTaiKhoan);
                ModelState.AddModelError("", "❌ Có lỗi xảy ra, vui lòng thử lại!");
                return View("SuaThongTin", model);
            }
        }

    }
}