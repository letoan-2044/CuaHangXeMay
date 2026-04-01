using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 XEM DANH SÁCH - PUBLIC
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Where(s => s.TrangThai == true)
                .OrderByDescending(s => s.Gia)
                .ToListAsync();

            SetUserInfo();
            return View(sanPhams);
        }

        // 🔥 CHI TIẾT - PUBLIC
        public async Task<IActionResult> Details(int? id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(s => s.MaSanPham == id && s.TrangThai == true);

            if (sanPham == null) return NotFound();

            SetUserInfo();
            ViewBag.MaSanPham = sanPham.MaSanPham;
            return View(sanPham);
        }

        // 🔥 THÊM GIỎ HÀNG - BẮT LOGIN
        [HttpPost]
        public IActionResult AddToCart(int maSanPham)
        {
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(maTaiKhoanStr))
            {
                TempData["error"] = "🔒 Vui lòng đăng nhập để thêm giỏ hàng!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            TempData["success"] = $"✅ Đã thêm sản phẩm ID {maSanPham} vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        // 🔥 USER INFO HELPER
        private void SetUserInfo()
        {
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (!string.IsNullOrEmpty(maTaiKhoanStr) && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                var taiKhoan = _context.TaiKhoans
                    .Include(tk => tk.ChucVu)
                    .FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);

                if (taiKhoan != null && taiKhoan.TrangThai)
                {
                    HttpContext.Session.SetString("HoTen", taiKhoan.HoTen ?? taiKhoan.TenDangNhap);
                    HttpContext.Session.SetString("ChucVu", taiKhoan.ChucVu?.TenChucVu ?? "Khách hàng");
                    HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);

                    ViewBag.HoTen = taiKhoan.HoTen ?? taiKhoan.TenDangNhap;
                    ViewBag.ChucVu = taiKhoan.ChucVu?.TenChucVu ?? "Khách hàng";
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> QuanLySanPham()
        {
            // ✅ CHECK QUYỀN TRUY CẬP
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2) // 1=Admin, 2=Nhân viên
            {
                TempData["error"] = "❌ Bạn không có quyền truy cập trang quản lý!";
                return RedirectToAction("Index");
            }

            var sanPhams = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .OrderByDescending(s => s.MaSanPham)
                .ToListAsync();

            SetUserInfo();
            return View(sanPhams);
        }
        // 🔥 THÊM/SỬA - GET (Thêm vào cuối class)
        [HttpGet]
        public IActionResult ThemSanPham(int? id)
        {
            // CHECK QUYỀN
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền truy cập!";
                return RedirectToAction("Index");
            }

            ViewBag.DanhMucs = _context.DanhMucs.ToList(); // 🔥 LƯU Ý: DanhMucs (số nhiều)
            ViewBag.Title = id.HasValue ? $"Sửa sản phẩm - ID: {id}" : "Thêm sản phẩm mới";
            ViewBag.Action = "ThemSanPham";

            if (id.HasValue && id > 0)
            {
                var sanPham = _context.SanPhams.Include(s => s.DanhMuc)
                    .FirstOrDefault(s => s.MaSanPham == id);
                if (sanPham == null) return NotFound();
                return View(sanPham);
            }
            return View(new SanPham { TrangThai = true });
        }

        // 🔥 THÊM/SỬA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSanPham(int id, SanPham sanPham, IFormFile? hinhAnh, string? hinhAnhCu)
        {
            // CHECK QUYỀN
            var maChucVu = HttpContext.Session.GetInt32("MaChucVu");
            if (maChucVu != 1 && maChucVu != 2)
            {
                TempData["error"] = "❌ Bạn không có quyền thực hiện!";
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

                    // Xử lý ảnh
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
                else // THÊM
                {
                    if (hinhAnh != null && hinhAnh.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(hinhAnh.FileName)}";
                        var filePath = Path.Combine("wwwroot/images", fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await hinhAnh.CopyToAsync(stream);
                        sanPham.HinhAnh = fileName;
                    }

                    _context.SanPhams.Add(sanPham);
                    TempData["success"] = "✅ Thêm thành công!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("QuanLySanPham");
            }

            ViewBag.DanhMucs = _context.DanhMucs.ToList();
            ViewBag.Title = id > 0 ? $"Sửa sản phẩm - ID: {id}" : "Thêm sản phẩm mới";
            ViewBag.Action = "ThemSanPham";
            return View(sanPham);
        }

        // 🔥 XÓA
        public async Task<IActionResult> XoaSanPham(int id)
        {
            // CHECK QUYỀN
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
    }
}