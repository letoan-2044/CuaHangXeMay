using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data;
using WebsiteBanXeMay.Models;
using Newtonsoft.Json; 
namespace WebsiteBanXeMay.Controllers
{
    public class DonHangController : Controller
    {
        private readonly AppDbContext _context;

        public DonHangController(AppDbContext context)
        {
            _context = context;
        }

        //  LỊCH SỬ KHÁCH HÀNG
        public async Task<IActionResult> Index(string trangThai = "", int trang = 1)
        {
            var maTK = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTK.HasValue || maTK == 0)
            {
                //  FIX 2: Lưu URL (tùy chọn)
                HttpContext.Session.SetString("RedirectUrl", $"/DonHang?trangThai={trangThai}&trang={trang}");
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            //  Include từng bước
            var query = _context.DonHangs
                .Where(d => d.MaTaiKhoan == maTK);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            int pageSize = 8;
            var total = await query.CountAsync();

            var donHangs = await query
                .OrderByDescending(d => d.NgayDat)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.ChiTietDonHangs)
                .ToListAsync();

            ViewBag.CurrentPage = trang;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TrangThai = trangThai;
            return View(donHangs);
        }

        //  QUẢN LÝ 
        public async Task<IActionResult> QuanLyDonHang(string trangThai = "", string tuKhoa = "", int trang = 1)
        {
            var chucVu = HttpContext.Session.GetString("ChucVu");
            if (chucVu != "Admin" && chucVu != "Nhân viên")
            {
                TempData["error"] = "❌ Không có quyền!";
                return RedirectToAction("Index", "Home");
            }

            var query = _context.DonHangs.AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(d =>
                    d.MaDonHang.ToString().Contains(tuKhoa) ||
                    d.SoDienThoai.Contains(tuKhoa));

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            int pageSize = 10;
            var total = await query.CountAsync();

            var donHangs = await query
                .OrderByDescending(d => d.NgayDat)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonHangs)
                .ToListAsync();

            ViewBag.CurrentPage = trang;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TrangThai = trangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TongDoanhThu = donHangs.Sum(d => d.TongTien ?? 0);
            return View(donHangs);
        }

        //  CHI TIẾT
        public async Task<IActionResult> ChiTiet(int id)
        {
            // Lay don hang theo MaDonHang
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs!)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                TempData["Loi"] = "Khong tim thay don hang";
                return RedirectToAction("Index");
            }

            var model = new DatHangViewModel
            {
                DonHang = donHang,
                ChiTietGioHangs = donHang.ChiTietDonHangs.Select(ct => new ChiTietGioHang
                {
                    SanPham = ct.SanPham,
                    SoLuong = ct.SoLuong,
                     GiaBan = ct.GiaBan
                }).ToList(),
                TongTien = donHang.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.GiaBan)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> CapNhatTrangThai(int maDonHang, string trangThaiMoi)
        {
            // 🔥 DEBUG: Log giá trị nhận được
            Console.WriteLine($"DEBUG: maDonHang={maDonHang}, trangThaiMoi='{trangThaiMoi}'");

            var chucVu = HttpContext.Session.GetString("ChucVu");
            if (chucVu != "Admin" && chucVu != "Nhân viên")
                return Json(new { success = false, message = "❌ Không có quyền!" });

            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null)
                return Json(new { success = false, message = "❌ Không tìm thấy!" });

            var trangThaiHopLe = new[] {
        "Chờ xử lý", "Đang giao", "Đã giao", "Hủy", "Bị thông báo chưa hợp lệ"
    };

            // 🔥 DEBUG: Kiểm tra chính xác
            Console.WriteLine($"DEBUG: trangThaiMoi='{trangThaiMoi?.Trim()}'");
            Console.WriteLine($"DEBUG: Contains? {trangThaiHopLe.Contains(trangThaiMoi?.Trim())}");

            if (!trangThaiHopLe.Contains(trangThaiMoi?.Trim()))
            {
                Console.WriteLine($"DEBUG: Valid states: [{string.Join(", ", trangThaiHopLe)}]");
                return Json(new
                {
                    success = false,
                    message = $"❌ Trạng thái không hợp lệ! Nhận được: '{trangThaiMoi}'"
                });
            }

            if (donHang.TrangThai == "Đã giao" || donHang.TrangThai == "Hủy")
                return Json(new { success = false, message = "❌ Đã hoàn thành!" });

            donHang.TrangThai = trangThaiMoi.Trim();
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"✅ Cập nhật: {trangThaiMoi.Trim()}", trangThai = trangThaiMoi.Trim() });
        }
        // 🔥 THỐNG KÊ
        [HttpGet]
        public async Task<IActionResult> ThongKe()
        {
            var stats = new
            {
                TongDonHang = await _context.DonHangs.CountAsync(),
                ChoXuLy = await _context.DonHangs.CountAsync(d => d.TrangThai == "Chờ xử lý"),
                DangGiao = await _context.DonHangs.CountAsync(d => d.TrangThai == "Đang giao"),
                TongDoanhThu = await _context.DonHangs.Where(d => d.TrangThai == "Đã giao").SumAsync(d => d.TongTien ?? 0)
            };
            return Json(stats);
        }
        // 🔥 THÊM VÀO DonHangController.cs
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ThongKeDonHang()
        {
            var chucVu = HttpContext.Session.GetString("ChucVu");
            if (chucVu != "Admin" && chucVu != "Nhân viên")
                return Json(new { success = false, message = "❌ Không có quyền!" });

            var query = _context.DonHangs.AsQueryable();

            var tuKhoa = Request.Query["tuKhoa"].ToString();
            var trangThai = Request.Query["trangThai"].ToString();

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(d => d.MaDonHang.ToString().Contains(tuKhoa) || d.SoDienThoai.Contains(tuKhoa));

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            var orders = await query
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            // 🔥 DOANH THU: CHỈ TÍNH ĐƠN KHÔNG HỦY
            var doanhThu = orders
                .Where(d => d.TrangThai != "Hủy")
                .Sum(d => d.TongTien ?? 0);

            var stats = new
            {
                totalOrders = orders.Count,
                totalRevenue = doanhThu,  // ✅ ĐÚNG
                statusCount = orders.GroupBy(d => d.TrangThai)
                    .ToDictionary(g => g.Key ?? "Không xác định", g => g.Count()),
                orders = orders.Select(o => new
                {
                    maDH = o.MaDonHang,
                    tenKH = o.TaiKhoan?.HoTen ?? "N/A",
                    sdt = o.SoDienThoai ?? "",
                    ngayDat = o.NgayDat.ToString("dd/MM/yyyy HH:mm"),
                    tongTien = o.TongTien ?? 0,
                    trangThai = o.TrangThai ?? "Chờ xử lý"
                }).ToList()
            };

            return Json(stats);
        }
    }
}
