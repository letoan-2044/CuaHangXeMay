using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data;
using WebsiteBanXeMay.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebsiteBanXeMay.Controllers
{
    public class GioHangController : Controller
    {
        private readonly AppDbContext _context;

        public GioHangController(AppDbContext context)
        {
            _context = context;
        }

        private void SetUserInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                var identityName = User.Identity.Name;
                if (!string.IsNullOrEmpty(identityName))
                {
                    var parts = identityName.Split(',');
                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int maTaiKhoanFromIdentity))
                    {
                        HttpContext.Session.SetInt32("MaTaiKhoan", maTaiKhoanFromIdentity);

                        var taiKhoan = _context.TaiKhoans
                            .Include(tk => tk.ChucVu)
                            .FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoanFromIdentity);

                        if (taiKhoan != null && taiKhoan.TrangThai)
                        {
                            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen ?? taiKhoan.TenDangNhap);
                            HttpContext.Session.SetString("ChucVu", taiKhoan.ChucVu?.TenChucVu ?? "Khách hàng");
                            HttpContext.Session.SetInt32("MaChucVu", taiKhoan.MaChucVu);
                            return;
                        }
                    }
                }
            }

            int? sessionMaTK = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (sessionMaTK.HasValue)
            {
                var taiKhoan = _context.TaiKhoans
                    .Include(tk => tk.ChucVu)
                    .FirstOrDefault(tk => tk.MaTaiKhoan == sessionMaTK.Value);

                if (taiKhoan != null && taiKhoan.TrangThai)
                {
                    HttpContext.Session.SetString("HoTen", taiKhoan.HoTen ?? taiKhoan.TenDangNhap);
                    HttpContext.Session.SetString("ChucVu", taiKhoan.ChucVu?.TenChucVu ?? "Khách hàng");
                    HttpContext.Session.SetInt32("MaChucVu", taiKhoan.MaChucVu);
                }
            }
        }

        private int? GetCurrentUserId()
        {
            var sessionId = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (sessionId.HasValue) return sessionId.Value;

            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId)
                ? userId : null;
        }

        // 🔥 FIX: Đổi tên TẤT CẢ thành cartGioHang
        public async Task<IActionResult> Index()
        {
            SetUserInfo();

            var cartGioHang = await GetGioHangAsync();
            if (cartGioHang == null)
            {
                ViewBag.TongTien = 0m;
                ViewBag.SoLuongSanPham = 0;
                return View(new List<ChiTietGioHang>());
            }

            var chiTietGioHangs = await _context.ChiTietGioHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.MaGioHang == cartGioHang.MaGioHang)
                .ToListAsync();

            ViewBag.TongTien = await TongTienGioHang(cartGioHang.MaGioHang);
            ViewBag.SoLuongSanPham = chiTietGioHangs.Sum(ct => ct.SoLuong);
            ViewBag.DebugUserId = GetCurrentUserId();
            ViewBag.DebugGioHangId = cartGioHang.MaGioHang;

            return View(chiTietGioHangs);
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGioHang(int maSanPham, int soLuong = 1,
            string tuKhoa = "", string hangXe = "", int page = 1)
        {
            SetUserInfo();

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["Loi"] = "❌ Vui lòng đăng nhập để thêm vào giỏ hàng!";
                return RedirectToAction("DangNhap", "TaiKhoan",
                    new { returnUrl = $"/SanPham/Details/{maSanPham}" });
            }

            var cartGioHang = await GetGioHangAsync();
            if (cartGioHang == null)
            {
                cartGioHang = new GioHang { MaTaiKhoan = userId };
                _context.GioHangs.Add(cartGioHang);
                await _context.SaveChangesAsync();
            }

            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null || sanPham.SoLuongTon < soLuong)
            {
                TempData["Loi"] = "Sản phẩm không tồn tại hoặc không đủ hàng trong kho!";
                return RedirectToAction("Index");
            }

            var chiTietTonTai = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaGioHang == cartGioHang.MaGioHang && ct.MaSanPham == maSanPham);

            if (chiTietTonTai != null)
            {
                if (chiTietTonTai.SoLuong + soLuong > sanPham.SoLuongTon)
                {
                    TempData["Loi"] = $"Số lượng vượt quá tồn kho! Còn {sanPham.SoLuongTon} sp";
                    return RedirectToAction("Index");
                }
                chiTietTonTai.SoLuong += soLuong;
            }
            else
            {
                _context.ChiTietGioHangs.Add(new ChiTietGioHang
                {
                    MaGioHang = cartGioHang.MaGioHang,
                    MaSanPham = maSanPham,
                    SoLuong = soLuong
                });
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.SetInt32("SoLuongGioHang", await TongSoLuongGioHangAsync());

            TempData["ThanhCong"] = $"✅ Đã thêm {soLuong} x {sanPham.TenSanPham} vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatSoLuong(int maChiTietGio, int soLuong)
        {
            SetUserInfo();

            if (soLuong <= 0)
            {
                return await Xoa(maChiTietGio);
            }

            var chiTiet = await _context.ChiTietGioHangs
                .Include(ct => ct.SanPham)
                .Include(ct => ct.GioHang)
                .FirstOrDefaultAsync(ct => ct.MaChiTietGio == maChiTietGio);

            if (chiTiet != null && chiTiet.GioHang.MaTaiKhoan == GetCurrentUserId())
            {
                if (soLuong <= chiTiet.SanPham.SoLuongTon)
                {
                    chiTiet.SoLuong = soLuong;
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetInt32("SoLuongGioHang", await TongSoLuongGioHangAsync());
                    TempData["ThanhCong"] = "✅ Đã cập nhật giỏ hàng!";
                }
                else
                {
                    TempData["Loi"] = $"❌ Số lượng vượt quá tồn kho! Còn {chiTiet.SanPham.SoLuongTon} sp";
                }
            }
            else
            {
                TempData["Loi"] = "❌ Không tìm thấy sản phẩm trong giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int maChiTietGio)
        {
            SetUserInfo();

            var chiTiet = await _context.ChiTietGioHangs
                .Include(ct => ct.GioHang)
                .FirstOrDefaultAsync(ct => ct.MaChiTietGio == maChiTietGio);

            if (chiTiet != null && chiTiet.GioHang.MaTaiKhoan == GetCurrentUserId())
            {
                _context.ChiTietGioHangs.Remove(chiTiet);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetInt32("SoLuongGioHang", await TongSoLuongGioHangAsync());
                TempData["ThanhCong"] = "✅ Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTatCa()
        {
            SetUserInfo();

            var cartGioHang = await GetGioHangAsync();
            if (cartGioHang != null)
            {
                var chiTietGioHangs = _context.ChiTietGioHangs
                    .Where(ct => ct.MaGioHang == cartGioHang.MaGioHang);
                _context.ChiTietGioHangs.RemoveRange(chiTietGioHangs);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetInt32("SoLuongGioHang", 0);
                TempData["ThanhCong"] = "🗑️ Đã xóa toàn bộ giỏ hàng!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<int> GetSoLuongGioHang()
        {
            SetUserInfo();
            var tempCart = await GetGioHangAsync(); // ✅ Tên khác hẳn
            if (tempCart == null) return 0;
            return await _context.ChiTietGioHangs
                .Where(ct => ct.MaGioHang == tempCart.MaGioHang)
                .SumAsync(ct => ct.SoLuong);
        }

        private async Task<GioHang?> GetGioHangAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;

            return await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaTaiKhoan == userId);
        }

        private async Task<int> TongSoLuongGioHangAsync()
        {
            var tempCart = await GetGioHangAsync(); // ✅ Tên khác hẳn
            if (tempCart == null) return 0;
            return await _context.ChiTietGioHangs
                .Where(ct => ct.MaGioHang == tempCart.MaGioHang)
                .SumAsync(ct => ct.SoLuong);
        }

        private async Task<decimal> TongTienGioHang(int maGioHang)
        {
            return await _context.ChiTietGioHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.MaGioHang == maGioHang)
                .SumAsync(ct => ct.SoLuong * ct.SanPham.Gia);
        }

        [HttpGet]
        public async Task<IActionResult> DatHang()
        {
            SetUserInfo();

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["Loi"] = "❌ Vui lòng đăng nhập để đặt hàng!";
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = "/GioHang/Index" });
            }

            var cartGioHang = await GetGioHangAsync();
            if (cartGioHang == null || !await _context.ChiTietGioHangs.AnyAsync(ct => ct.MaGioHang == cartGioHang.MaGioHang))
            {
                TempData["Loi"] = "❌ Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            var chiTietGioHangs = await _context.ChiTietGioHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.MaGioHang == cartGioHang.MaGioHang)
                .ToListAsync();

            var datHangViewModel = new DatHangViewModel
            {
                ChiTietGioHangs = chiTietGioHangs,
                TongTien = chiTietGioHangs.Sum(ct => ct.SoLuong * ct.SanPham.Gia),
                DonHang = new DonHang()
            };

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(tk => tk.MaTaiKhoan == userId);
            if (taiKhoan != null)
            {
                datHangViewModel.DonHang.SoDienThoai = taiKhoan.SoDienThoai;
            }

            ViewBag.SoLuongSanPham = chiTietGioHangs.Sum(ct => ct.SoLuong);
            return View(datHangViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHang(DatHangViewModel model)
        {
            SetUserInfo();

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (!ModelState.IsValid)
            {
                var tempCart = await GetGioHangAsync();
                if (tempCart != null)
                {
                    model.ChiTietGioHangs = await _context.ChiTietGioHangs
                        .Include(ct => ct.SanPham)
                        .Where(ct => ct.MaGioHang == tempCart.MaGioHang)
                        .ToListAsync();
                    model.TongTien = model.ChiTietGioHangs.Sum(ct => ct.SoLuong * ct.SanPham.Gia);
                    ViewBag.SoLuongSanPham = model.ChiTietGioHangs.Sum(ct => ct.SoLuong);
                }
                return View(model);
            }

            var checkoutCart = await GetGioHangAsync();
            if (checkoutCart == null)
            {
                TempData["Loi"] = "❌ Giỏ hàng không tồn tại!";
                return RedirectToAction("Index");
            }
            var cartItems = await _context.ChiTietGioHangs  
    .Include(ct => ct.SanPham)
    .Where(ct => ct.MaGioHang == checkoutCart.MaGioHang)
    .ToListAsync();
            var donHang = new DonHang
            {
                MaTaiKhoan = userId,
                NgayDat = DateTime.Now,
                TongTien = cartItems.Sum(ct => ct.SoLuong * ct.SanPham.Gia),  
                DiaChiGiaoHang = model.DonHang.DiaChiGiaoHang,
                SoDienThoai = model.DonHang.SoDienThoai,
                TrangThai = "Chờ xử lý",
                
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            var chiTietGioHangs = await _context.ChiTietGioHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.MaGioHang == checkoutCart.MaGioHang)
                .ToListAsync();

            foreach (var ctgh in chiTietGioHangs)
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSanPham = ctgh.MaSanPham,
                    SoLuong = ctgh.SoLuong,
                    GiaBan = ctgh.SanPham.Gia
                });

                // 🔥 Cập nhật tồn kho
                ctgh.SanPham.SoLuongTon -= ctgh.SoLuong;
            }

            // 🔥 XÓA TOÀN BỘ CHI TIẾT GIỎ HÀNG
            _context.ChiTietGioHangs.RemoveRange(chiTietGioHangs);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("SoLuongGioHang", 0);

            TempData["ThanhCong"] = "ĐẶT HÀNG THÀNH CÔNG!";
            TempData["MaDonHang"] = donHang.MaDonHang.ToString();
            TempData["SoDienThoai"] = model.DonHang.SoDienThoai;

            return RedirectToAction("Index"); // 🔥 QUAY VỀ GIỎ HÀNG thay vì ChiTiet
        }
    }
}