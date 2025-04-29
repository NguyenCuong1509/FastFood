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
    [Authorize]
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GioHangController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Phương thức kiểm tra EmailConfirmed
        private async Task<IActionResult> CheckEmailConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginUser", "Account");
            }

            if (!user.EmailConfirmed)
            {
                TempData["ErrorMessage"] = "Vui lòng xác nhận email trước khi sử dụng giỏ hàng.";
                return RedirectToAction("EmailNotConfirmed", "Account"); // Chuyển hướng đến trang thông báo
            }

            return null; // Trả về null nếu email đã được xác nhận
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs).ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs).ThenInclude(cg => cg.Combo)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    UserId = userId,
                    MonAnGioHangs = new List<MonAnGioHang>(),
                    ComboGioHangs = new List<ComboGioHang>()
                };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            return View(gioHang);
        }

        // Thêm món ăn vào giỏ hàng
        public async Task<IActionResult> ThemVaoGio(int monAnId)
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var monAn = await _context.MonAns.FindAsync(monAnId);
            if (monAn == null)
            {
                TempData["ErrorMessage"] = "Món ăn không tồn tại!";
                return RedirectToAction("Index", "Home");
            }

            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang { UserId = userId, MonAnGioHangs = new List<MonAnGioHang>() };
                _context.GioHangs.Add(gioHang);
            }

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

        // Thêm combo vào giỏ hàng
        public async Task<IActionResult> ThemComboVaoGio(int comboId)
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var combo = await _context.Combos.FindAsync(comboId);
            if (combo == null)
            {
                TempData["ErrorMessage"] = "Combo không tồn tại!";
                return RedirectToAction("Index");
            }

            var gioHang = await _context.GioHangs
                .Include(g => g.ComboGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang { UserId = userId, ComboGioHangs = new List<ComboGioHang>() };
                _context.GioHangs.Add(gioHang);
            }

            var comboGioHang = gioHang.ComboGioHangs.FirstOrDefault(c => c.ComboId == comboId);
            if (comboGioHang != null)
            {
                comboGioHang.SoLuong++;
            }
            else
            {
                gioHang.ComboGioHangs.Add(new ComboGioHang { ComboId = comboId, SoLuong = 1 });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        public async Task<IActionResult> CapNhatGio(int? monAnId, int? comboId, int soLuong)
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .Include(g => g.ComboGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                return NotFound("Không tìm thấy giỏ hàng.");
            }

            if (monAnId.HasValue)
            {
                var monAnGioHang = gioHang.MonAnGioHangs.FirstOrDefault(m => m.MonAnId == monAnId);
                if (monAnGioHang != null)
                {
                    monAnGioHang.SoLuong = soLuong > 0 ? soLuong : 1;
                }
            }
            else if (comboId.HasValue)
            {
                var comboGioHang = gioHang.ComboGioHangs.FirstOrDefault(c => c.ComboId == comboId);
                if (comboGioHang != null)
                {
                    comboGioHang.SoLuong = soLuong > 0 ? soLuong : 1;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Xóa món ăn khỏi giỏ hàng
        public async Task<IActionResult> XoaKhoiGio(int monAnId)
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        // Xóa combo khỏi giỏ hàng
        public async Task<IActionResult> XoaComboKhoiGio(int comboId)
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        // Xóa toàn bộ giỏ hàng
        public async Task<IActionResult> XoaTatCa()
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs)
                .Include(g => g.ComboGioHangs)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang != null)
            {
                gioHang.MonAnGioHangs.Clear();
                gioHang.ComboGioHangs.Clear();
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Thanh toán giỏ hàng
        public async Task<IActionResult> ThanhToan()
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs).ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs).ThenInclude(cg => cg.Combo)
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

            _context.MonAnGioHangs.RemoveRange(gioHang.MonAnGioHangs);
            _context.ComboGioHangs.RemoveRange(gioHang.ComboGioHangs);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng của bạn đang chờ xác nhận.";
            return RedirectToAction("Index");
        }

        // Hiển thị đơn hàng của tôi
        public async Task<IActionResult> DonHangCuaToi()
        {
            // Kiểm tra EmailConfirmed
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var donHangs = await _context.HoaDons
                .Where(h => h.UserId == userId)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.MonAn)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.Combo)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            return View(donHangs);
        }
    }
}