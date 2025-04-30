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
                return RedirectToAction("EmailNotConfirmed", "Account");
            }

            return null;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs).ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs).ThenInclude(cg => cg.Combo).ThenInclude(c => c.MonAnCombos).ThenInclude(mc => mc.MonAn)
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
                if (monAnGioHang.SoLuong + 1 > monAn.SoLuongTonKho)
                {
                    TempData["ErrorMessage"] = $"Không thể thêm món {monAn.TenMon}. Số lượng trong kho không đủ!";
                    return RedirectToAction("Index");
                }
                monAnGioHang.SoLuong++;
            }
            else
            {
                if (monAn.SoLuongTonKho < 1)
                {
                    TempData["ErrorMessage"] = $"Không thể thêm món {monAn.TenMon}. Hết hàng!";
                    return RedirectToAction("Index");
                }
                gioHang.MonAnGioHangs.Add(new MonAnGioHang { MonAnId = monAnId, SoLuong = 1 });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Thêm combo vào giỏ hàng
        public async Task<IActionResult> ThemComboVaoGio(int comboId)
        {
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var combo = await _context.Combos
                .Include(c => c.MonAnCombos).ThenInclude(mc => mc.MonAn)
                .FirstOrDefaultAsync(c => c.ComboId == comboId);
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
            int newSoLuong = (comboGioHang?.SoLuong ?? 0) + 1;

            // Kiểm tra tồn kho cho các món ăn trong combo
            foreach (var monAnCombo in combo.MonAnCombos)
            {
                var monAn = monAnCombo.MonAn;
                int soLuongCan = newSoLuong * monAnCombo.SoLuong;
                if (monAn.SoLuongTonKho < soLuongCan)
                {
                    TempData["ErrorMessage"] = $"Không thể thêm combo {combo.TenCombo}. Món {monAn.TenMon} không đủ số lượng!";
                    return RedirectToAction("Index");
                }
            }

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
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs).ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs).ThenInclude(cg => cg.Combo).ThenInclude(c => c.MonAnCombos).ThenInclude(mc => mc.MonAn)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                return NotFound("Không tìm thấy giỏ hàng.");
            }

            if (soLuong < 1)
            {
                TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction("Index");
            }

            if (monAnId.HasValue)
            {
                var monAnGioHang = gioHang.MonAnGioHangs.FirstOrDefault(m => m.MonAnId == monAnId);
                if (monAnGioHang != null)
                {
                    var monAn = monAnGioHang.MonAn;
                    if (soLuong > monAn.SoLuongTonKho)
                    {
                        TempData["ErrorMessage"] = $"Số lượng món {monAn.TenMon} vượt quá tồn kho ({monAn.SoLuongTonKho}).";
                        return RedirectToAction("Index");
                    }
                    monAnGioHang.SoLuong = soLuong;
                }
            }
            else if (comboId.HasValue)
            {
                var comboGioHang = gioHang.ComboGioHangs.FirstOrDefault(c => c.ComboId == comboId);
                if (comboGioHang != null)
                {
                    var combo = comboGioHang.Combo;
                    foreach (var monAnCombo in combo.MonAnCombos)
                    {
                        var monAn = monAnCombo.MonAn;
                        int soLuongCan = soLuong * monAnCombo.SoLuong;
                        if (soLuongCan > monAn.SoLuongTonKho)
                        {
                            TempData["ErrorMessage"] = $"Số lượng combo {combo.TenCombo} vượt quá tồn kho của món {monAn.TenMon} ({monAn.SoLuongTonKho}).";
                            return RedirectToAction("Index");
                        }
                    }
                    comboGioHang.SoLuong = soLuong;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        // Xóa món ăn khỏi giỏ hàng
        public async Task<IActionResult> XoaKhoiGio(int monAnId)
        {
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
            var emailCheckResult = await CheckEmailConfirmed();
            if (emailCheckResult != null) return emailCheckResult;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gioHang = await _context.GioHangs
                .Include(g => g.MonAnGioHangs).ThenInclude(mg => mg.MonAn)
                .Include(g => g.ComboGioHangs).ThenInclude(cg => cg.Combo).ThenInclude(c => c.MonAnCombos).ThenInclude(mc => mc.MonAn)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null || (gioHang.MonAnGioHangs.Count == 0 && gioHang.ComboGioHangs.Count == 0))
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra tồn kho cho món ăn
                foreach (var monAnGioHang in gioHang.MonAnGioHangs)
                {
                    var monAn = monAnGioHang.MonAn;
                    if (monAn.SoLuongTonKho < monAnGioHang.SoLuong)
                    {
                        TempData["ErrorMessage"] = $"Món ăn {monAn.TenMon} không đủ số lượng trong kho!";
                        await transaction.RollbackAsync();
                        return RedirectToAction("Index");
                    }
                }

                // Kiểm tra tồn kho cho món ăn trong combo
                foreach (var comboGioHang in gioHang.ComboGioHangs)
                {
                    var combo = comboGioHang.Combo;
                    foreach (var monAnCombo in combo.MonAnCombos)
                    {
                        var monAn = monAnCombo.MonAn;
                        int soLuongCan = comboGioHang.SoLuong * monAnCombo.SoLuong;
                        if (monAn.SoLuongTonKho < soLuongCan)
                        {
                            TempData["ErrorMessage"] = $"Món ăn {monAn.TenMon} trong combo {combo.TenCombo} không đủ số lượng!";
                            await transaction.RollbackAsync();
                            return RedirectToAction("Index");
                        }
                    }
                }

                // Trừ tồn kho
                foreach (var monAnGioHang in gioHang.MonAnGioHangs)
                {
                    var monAn = monAnGioHang.MonAn;
                    monAn.SoLuongTonKho -= monAnGioHang.SoLuong;
                }

                foreach (var comboGioHang in gioHang.ComboGioHangs)
                {
                    var combo = comboGioHang.Combo;
                    foreach (var monAnCombo in combo.MonAnCombos)
                    {
                        var monAn = monAnCombo.MonAn;
                        monAn.SoLuongTonKho -= comboGioHang.SoLuong * monAnCombo.SoLuong;
                    }
                }

                // Tính tổng tiền
                decimal tongTien = gioHang.MonAnGioHangs.Sum(mg => mg.MonAn.Gia * mg.SoLuong)
                                 + gioHang.ComboGioHangs.Sum(cg => cg.Combo.GiaKhuyenMai * cg.SoLuong);

                // Tạo hóa đơn
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

                // Lưu hóa đơn
                _context.HoaDons.Add(hoaDon);

                // Xóa giỏ hàng
                _context.MonAnGioHangs.RemoveRange(gioHang.MonAnGioHangs);
                _context.ComboGioHangs.RemoveRange(gioHang.ComboGioHangs);

                // Lưu tất cả thay đổi
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng của bạn đang chờ xác nhận.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán. Vui lòng thử lại!";
                return RedirectToAction("Index");
            }
        }

        // Hiển thị đơn hàng của tôi
        public async Task<IActionResult> DonHangCuaToi()
        {
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