using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FastFoodOnline.Controllers
{
    [Authorize] // Chỉ cho phép người dùng đã đăng nhập
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GioHangController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs).ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs).ThenInclude(cg => cg.Combo) // Thêm Include này
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    UserId = userId,
                    MonAnGioHangs = new List<MonAnGioHang>(),
                    ComboGioHangs = new List<ComboGioHang>() // Khởi tạo danh sách rỗng
                };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            return View(gioHang);
        }


        // Thêm món ăn vào giỏ hàng
        public async Task<IActionResult> ThemVaoGio(int monAnId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            // Kiểm tra món ăn có tồn tại không
            var monAn = await _context.MonAns.FindAsync(monAnId);
            if (monAn == null)
            {
                TempData["ErrorMessage"] = "Món ăn không tồn tại!";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra giỏ hàng của user
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang { UserId = userId, MonAnGioHangs = new List<MonAnGioHang>() };
                _context.GioHangs.Add(gioHang);
            }

            // Kiểm tra món ăn đã có trong giỏ hàng chưa
            var monAnGioHang = gioHang.MonAnGioHangs.FirstOrDefault(m => m.MonAnId == monAnId);
            if (monAnGioHang != null)
            {
                monAnGioHang.SoLuong++;
            }
            else
            {
                gioHang.MonAnGioHangs.Add(new MonAnGioHang { MonAnId = monAnId, SoLuong = 1 });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng món ăn
        [HttpPost]
        public async Task<IActionResult> CapNhatGio(int monAnId, int soLuong)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang != null)
            {
                var monAnGioHang = gioHang.MonAnGioHangs.FirstOrDefault(m => m.MonAnId == monAnId);
                if (monAnGioHang != null)
                {
                    monAnGioHang.SoLuong = soLuong > 0 ? soLuong : 1;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }


        // Xóa một món ăn khỏi giỏ hàng
        public async Task<IActionResult> XoaKhoiGio(int monAnId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang != null)
            {
                var monAnGioHang = gioHang.MonAnGioHangs.FirstOrDefault(m => m.MonAnId == monAnId);
                if (monAnGioHang != null)
                {
                    gioHang.MonAnGioHangs.Remove(monAnGioHang);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public async Task<IActionResult> XoaTatCa()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang != null)
            {
                gioHang.MonAnGioHangs.Clear();
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
        // Thanh toán giỏ hàng
        public async Task<IActionResult> ThanhToan()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs)
                .ThenInclude(cg => cg.Combo)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null || (gioHang.MonAnGioHangs.Count == 0 && gioHang.ComboGioHangs.Count == 0))
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            decimal tongTien = gioHang.MonAnGioHangs.Sum(mg => mg.MonAn.Gia * mg.SoLuong)
                             + gioHang.ComboGioHangs.Sum(cg => cg.Combo.GiaKhuyenMai * cg.SoLuong);

            var hoaDon = new HoaDon
            {
                UserId = userId,
                NgayTao = DateTime.Now,
                TongTien = tongTien,
                TrangThai = TrangThaiHoaDon.ChoXacNhan,
                HoaDonChiTiets = gioHang.MonAnGioHangs.Select(mg => new HoaDonChiTiet
                {
                    MonAnId = mg.MonAnId,
                    SoLuong = mg.SoLuong,
                    Gia = mg.MonAn.Gia
                }).ToList()
            };

            // Thêm chi tiết đơn hàng cho combo
            foreach (var comboGioHang in gioHang.ComboGioHangs)
            {
                hoaDon.HoaDonChiTiets.Add(new HoaDonChiTiet
                {
                    ComboId = comboGioHang.ComboId,
                    SoLuong = comboGioHang.SoLuong,
                    Gia = comboGioHang.Combo.GiaKhuyenMai
                });
            }

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi thanh toán thành công
            _context.MonAnGioHangs.RemoveRange(gioHang.MonAnGioHangs);
            _context.ComboGioHangs.RemoveRange(gioHang.ComboGioHangs);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng của bạn đang chờ xác nhận.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DonHangCuaToi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var donHangs = await _context.HoaDons
        .Where(h => h.UserId == userId)
        .Include(h => h.HoaDonChiTiets)
            .ThenInclude(ct => ct.MonAn)
        .Include(h => h.HoaDonChiTiets)
            .ThenInclude(ct => ct.Combo) // Đảm bảo lấy thông tin Combo
        .ToListAsync();


            return View(donHangs);
        }
        public async Task<IActionResult> ThemComboVaoGio(int comboId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                Console.WriteLine("Người dùng chưa đăng nhập.");
                return RedirectToAction("LoginUser", "Account");
            }

            var combo = await _context.Combos.FindAsync(comboId);
            if (combo == null)
            {
                Console.WriteLine("Combo không tồn tại! ID: " + comboId);
                TempData["ErrorMessage"] = "Combo không tồn tại!";
                return RedirectToAction("Index", "GioHang");

            }

            var gioHang = await _context.GioHangs
                .Include(g => g.ComboGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang { UserId = userId, ComboGioHangs = new List<ComboGioHang>() };
                _context.GioHangs.Add(gioHang);
                Console.WriteLine("Giỏ hàng mới được tạo cho user " + userId);
            }

            if (gioHang.ComboGioHangs == null)
            {
                gioHang.ComboGioHangs = new List<ComboGioHang>();
            }

            var comboGioHang = gioHang.ComboGioHangs.FirstOrDefault(c => c.ComboId == comboId);
            if (comboGioHang != null)
            {
                comboGioHang.SoLuong++;
                Console.WriteLine("Tăng số lượng combo " + comboId);
            }
            else
            {
                gioHang.ComboGioHangs.Add(new ComboGioHang { ComboId = comboId, SoLuong = 1 });
                Console.WriteLine("Thêm combo mới vào giỏ: " + comboId);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Lưu giỏ hàng thành công.");

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> CapNhatCombo(int comboId, int soLuong)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.ComboGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang != null)
            {
                var comboGioHang = gioHang.ComboGioHangs.FirstOrDefault(c => c.ComboId == comboId);
                if (comboGioHang != null)
                {
                    comboGioHang.SoLuong = soLuong > 0 ? soLuong : 1;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> XoaComboKhoiGio(int comboId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("LoginUser", "Account");

            var gioHang = await _context.GioHangs
                .Include(g => g.ComboGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang != null)
            {
                var comboGioHang = gioHang.ComboGioHangs.FirstOrDefault(c => c.ComboId == comboId);
                if (comboGioHang != null)
                {
                    gioHang.ComboGioHangs.Remove(comboGioHang);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

    }
}
