using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Extensions;
namespace WebsiteBanXeMay.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 TÌM KIẾM + LỌC HÃNG XE + PHÂN TRANG ✅ KHÔNG CẦN THAY ĐỔI DB
        public async Task<IActionResult> Index(string tuKhoa, string hangXe, int page = 1)
        {
            int pageSize = 12;

            // Base query
            var query = _context.SanPhams
                .Include(s => s.DanhMuc)
                .Where(s => s.TrangThai == true && s.Gia > 0 && s.SoLuongTon > 0)
                .AsQueryable();


            // 🔥 TÌM KIẾM THEO TỪ KHÓA (tên + mô tả)
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim().ToLower();
                query = query.Where(s =>
                    s.TenSanPham.ToLower().Contains(tuKhoa) ||
                    s.MoTa.ToLower().Contains(tuKhoa));
            }

            // 🔥 LỌC HÃNG XE (dựa vào tên sản phẩm - KHÔNG CẦN TRƯỜNG HangXe)
            if (!string.IsNullOrWhiteSpace(hangXe))
            {
                var upperHangXe = hangXe.Trim().ToUpper();
                query = query.Where(s => s.TenSanPham.ToUpper().Contains(upperHangXe));
            }

            // 🔥 PHÂN TRANG
            var totalItems = await query.CountAsync();
            var sanPhams = await query
                .OrderByDescending(s => s.Gia)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔥 VIEWBAG CHO LAYOUT DROPDOWN (KHÔNG LỖI RAZOR)
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.SelectedHangXe = hangXe;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;

            SetUserInfo();
            return View(sanPhams);
        }

        // 🔥 GỢI Ý TÌM KIẾM (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetSearchSuggestions(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa) || tuKhoa.Length < 2)
                return Json(new List<string>());

            var suggestions = await _context.SanPhams
                .Where(s => s.TrangThai == true && s.Gia > 0 && s.SoLuongTon > 0 &&
                           (s.TenSanPham.Contains(tuKhoa) ||
                            s.MoTa.Contains(tuKhoa)))
                .Select(s => s.TenSanPham)
                .Distinct()
                .Take(6)
                .ToListAsync();

            return Json(suggestions);
        }

        // 🔥 CHI TIẾT SẢN PHẨM
        public async Task<IActionResult> Details(int? id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(s => s.MaSanPham == id && s.TrangThai == true && s.Gia > 0 && s.SoLuongTon > 0);

            if (sanPham == null) return NotFound();

            SetUserInfo();
            ViewBag.MaSanPham = sanPham.MaSanPham;
            return View(sanPham);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int maSanPham, string tuKhoa = "", string hangXe = "", int page = 1)
        {
            // 🔥 SYNC USER TRƯỚC
            SetUserInfo();

            // 🔥 LẤY MA TÀI KHOẢN AN TOÀN
            int maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan") ?? 0;

            // 🔥 BACKUP IDENTITY
            if (maTaiKhoan == 0 && User.Identity.IsAuthenticated)
            {
                var identityName = User.Identity.Name;
                if (!string.IsNullOrEmpty(identityName))
                {
                    var parts = identityName.Split(',');
                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int userId))
                    {
                        maTaiKhoan = userId;
                        HttpContext.Session.SetInt32("MaTaiKhoan", maTaiKhoan);
                    }
                }
            }

            if (maTaiKhoan == 0)
            {
                TempData["error"] = "❌ Vui lòng đăng nhập!";
                return RedirectToAction("DangNhap", "TaiKhoan",
                    new { returnUrl = $"/SanPham/Index?tuKhoa={tuKhoa}&hangXe={hangXe}&page={page}" });
            }

            var sanPham = await _context.SanPhams.FindAsync(maSanPham);
            if (sanPham == null)
                return RedirectToAction("Index", new { tuKhoa, hangXe, page });

            // Tìm giỏ hàng
            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);

            if (gioHang == null)
            {
                gioHang = new GioHang { MaTaiKhoan = maTaiKhoan };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            // Thêm/Cập nhật chi tiết
            var chiTiet = gioHang.ChiTietGioHangs
                ?.FirstOrDefault(ct => ct.MaSanPham == maSanPham);

            if (chiTiet != null)
                chiTiet.SoLuong++;
            else
            {
                _context.ChiTietGioHangs.Add(new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSanPham = maSanPham,
                    SoLuong = 1
                });
            }

            await _context.SaveChangesAsync();

            // Cập nhật session
            var tongSoLuong = await _context.ChiTietGioHangs
                .Where(ct => ct.GioHang.MaTaiKhoan == maTaiKhoan)
                .SumAsync(ct => ct.SoLuong);
            HttpContext.Session.SetInt32("SoLuongGioHang", (int)tongSoLuong);

            TempData["success"] = $"✅ Đã thêm {sanPham.TenSanPham}!";
            return RedirectToAction("Index", new { tuKhoa, hangXe, page });
        }
        // 🔥 QUẢN LÝ SẢN PHẨM - ADMIN/NV
        [HttpGet]
        public async Task<IActionResult> QuanLySanPham()
        {
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền truy cập!";
                return RedirectToAction("Index");
            }

            var sanPhams = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .OrderByDescending(s => s.MaSanPham)
                .ToListAsync();

            SetUserInfo();
            return View(sanPhams);
        }

        // 🔥 THÊM/SỬA SẢN PHẨM - GET
        [HttpGet]
        public IActionResult ThemSanPham(int? id)
        {
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền!";
                return RedirectToAction("Index");
            }

            ViewBag.DanhMucs = _context.DanhMucs.ToList();
            ViewBag.Title = id.HasValue ? $"Sửa sản phẩm - ID: {id}" : "Thêm sản phẩm mới";

            if (id.HasValue && id > 0)
            {
                var sanPham = _context.SanPhams.Include(s => s.DanhMuc)
                    .FirstOrDefault(s => s.MaSanPham == id);
                if (sanPham == null) return NotFound();
                return View(sanPham);
            }
            return View(new SanPham { TrangThai = true });
        }

        // 🔥 THÊM/SỬA SẢN PHẨM - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSanPham(int id, SanPham sanPham, IFormFile? hinhAnh, string? hinhAnhCu)
        {
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                if (id > 0) // SỬA
                {
                    var existing = await _context.SanPhams.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.TenSanPham = sanPham.TenSanPham;
                    existing.MoTa = sanPham.MoTa;
                    existing.Gia = sanPham.Gia;
                    existing.SoLuongTon = sanPham.SoLuongTon;
                    existing.MaDanhMuc = sanPham.MaDanhMuc;
                    existing.TrangThai = sanPham.TrangThai;

                    // Upload ảnh mới
                    if (hinhAnh != null && hinhAnh.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(hinhAnhCu))
                        {
                            var oldPath = Path.Combine("wwwroot/images", hinhAnhCu);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(hinhAnh.FileName)}";
                        var filePath = Path.Combine("wwwroot/images", fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await hinhAnh.CopyToAsync(stream);
                        existing.HinhAnh = fileName;
                    }

                    TempData["success"] = "✅ Cập nhật thành công!";
                }
                else // THÊM MỚI
                {
                    if (hinhAnh == null || hinhAnh.Length == 0)
                    {
                        ModelState.AddModelError("", "❌ Vui lòng chọn ảnh sản phẩm!");
                        ViewBag.DanhMucs = _context.DanhMucs.ToList();
                        return View(sanPham);
                    }

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(hinhAnh.FileName)}";
                    var filePath = Path.Combine("wwwroot/images", fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await hinhAnh.CopyToAsync(stream);
                    sanPham.HinhAnh = fileName;
                    sanPham.TrangThai = true;

                    _context.SanPhams.Add(sanPham);
                    TempData["success"] = "✅ Thêm sản phẩm thành công!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("QuanLySanPham");
            }

            ViewBag.DanhMucs = _context.DanhMucs.ToList();
            return View(sanPham);
        }

        // 🔥 XÓA SẢN PHẨM
        public async Task<IActionResult> XoaSanPham(int id)
        {
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền xóa!";
                return RedirectToAction("Index");
            }

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                {
                    var imagePath = Path.Combine("wwwroot/images", sanPham.HinhAnh);
                    if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                }
                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();
                TempData["success"] = "🗑️ Xóa thành công!";
            }
            return RedirectToAction("QuanLySanPham");
        }

        // 🔥 USER INFO HELPER
        private void SetUserInfo()
        {
            // 🔥 SYNC IDENTITY → SESSION
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

                            int? maChucVu = taiKhoan.MaChucVu;
                            HttpContext.Session.SetInt32("MaChucVu", maChucVu ?? 3);

                            ViewBag.HoTen = taiKhoan.HoTen ?? taiKhoan.TenDangNhap;
                            ViewBag.ChucVu = taiKhoan.ChucVu?.TenChucVu ?? "Khách hàng";
                            ViewBag.IsLoggedIn = true;
                            return;
                        }
                    }
                }
            }

            // 🔥 SESSION FALLBACK
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

                    int? maChucVu = taiKhoan.MaChucVu;
                    HttpContext.Session.SetInt32("MaChucVu", maChucVu ?? 3);

                    ViewBag.HoTen = taiKhoan.HoTen ?? taiKhoan.TenDangNhap;
                    ViewBag.ChucVu = taiKhoan.ChucVu?.TenChucVu ?? "Khách hàng";
                    ViewBag.IsLoggedIn = true;
                    return;
                }
            }

            ViewBag.IsLoggedIn = false;
            ViewBag.HoTen = "Khách";
            ViewBag.ChucVu = "Khách hàng";
        }
    }
}