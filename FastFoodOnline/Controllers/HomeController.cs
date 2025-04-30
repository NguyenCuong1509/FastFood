using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using FastFoodOnline.Data;
using FastFoodOnline.Models;

namespace FastFoodOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;
            const int lowStockThreshold = 10;

            // Lấy danh sách món ăn
            var monAns = await _context.MonAns
                .Include(m => m.LoaiMonAn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách combo
            var combos = await _context.Combos
                .Include(c => c.MonAnCombos)
                .ThenInclude(mc => mc.MonAn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalMonAn = await _context.MonAns.CountAsync();
            int totalCombos = await _context.Combos.CountAsync();

            // Tính trạng thái
            var monAnTrangThai = monAns.ToDictionary(
                m => m.MonAnId,
                m => m.SoLuongTonKho switch
                {
                    0 => "Hết hàng",
                    < lowStockThreshold => "Sắp hết hàng",
                    _ => "Còn hàng"
                });

            var comboTrangThai = combos.ToDictionary(
                c => c.ComboId,
                c =>
                {
                    if (c.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho == 0))
                        return "Hết hàng";
                    if (c.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho < lowStockThreshold))
                        return "Sắp hết hàng";
                    return "Còn hàng";
                });

            var viewModel = new HomeViewModel
            {
                MonAns = monAns,
                Combos = combos,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalMonAn / pageSize),
                TotalPagesCombos = (int)Math.Ceiling((double)totalCombos / pageSize),
                MonAnTrangThai = monAnTrangThai,
                ComboTrangThai = comboTrangThai
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MonAnPartial", viewModel);
            }

            return View(viewModel);
        }

        public async Task<IActionResult> DanhSachSanPham(int page = 1, string searchQuery = null, int? loaiMonAnId = null)
        {
            int pageSize = 6;
            const int lowStockThreshold = 10;

            // Query cho món ăn
            var monAnQuery = _context.MonAns
                .Include(m => m.LoaiMonAn)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                monAnQuery = monAnQuery.Where(m => m.TenMon.Contains(searchQuery) || m.MoTa.Contains(searchQuery));
            }
            if (loaiMonAnId.HasValue)
            {
                monAnQuery = monAnQuery.Where(m => m.LoaiMonAnId == loaiMonAnId);
            }

            // Lấy danh sách món ăn liên quan đến combo
            var monAnComboIds = await _context.MonAnCombos.Select(x => x.MonAnId).Distinct().ToListAsync();
            monAnQuery = monAnQuery.Where(m => monAnComboIds.Contains(m.MonAnId));

            var monAnList = await monAnQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Query cho combo
            var comboQuery = _context.Combos
                .Include(c => c.MonAnCombos)
                .ThenInclude(mc => mc.MonAn)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                comboQuery = comboQuery.Where(c => c.TenCombo.Contains(searchQuery));
            }

            var comboList = await comboQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính trạng thái
            var monAnTrangThai = monAnList.ToDictionary(
                m => m.MonAnId,
                m => m.SoLuongTonKho switch
                {
                    0 => "Hết hàng",
                    < lowStockThreshold => "Sắp hết hàng",
                    _ => "Còn hàng"
                });

            var comboTrangThai = comboList.ToDictionary(
                c => c.ComboId,
                c =>
                {
                    if (c.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho == 0))
                        return "Hết hàng";
                    if (c.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho < lowStockThreshold))
                        return "Sắp hết hàng";
                    return "Còn hàng";
                });

            var viewModel = new HomeViewModel
            {
                MonAns = monAnList,
                Combos = comboList,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)await monAnQuery.CountAsync() / pageSize),
                TotalPagesCombos = (int)Math.Ceiling((double)await comboQuery.CountAsync() / pageSize),
                SearchQuery = searchQuery,
                LoaiMonAnId = loaiMonAnId,
                LoaiMonAns = await _context.LoaiMonAns.ToListAsync(),
                MonAnTrangThai = monAnTrangThai,
                ComboTrangThai = comboTrangThai
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MonAnPartial", viewModel);
            }

            return View(viewModel);
        }

        public IActionResult Search(string keyword)
        {
            const int lowStockThreshold = 10;

            var monAns = _context.MonAns
                .Include(m => m.LoaiMonAn)
                .Where(m => m.TenMon.Contains(keyword))
                .ToList();

            var combos = _context.Combos
                .Include(c => c.MonAnCombos)
                .ThenInclude(mc => mc.MonAn)
                .Where(c => c.TenCombo.Contains(keyword))
                .ToList();

            // Tính trạng thái
            var monAnTrangThai = monAns.ToDictionary(
                m => m.MonAnId,
                m => m.SoLuongTonKho switch
                {
                    0 => "Hết hàng",
                    < lowStockThreshold => "Sắp hết hàng",
                    _ => "Còn hàng"
                });

            var comboTrangThai = combos.ToDictionary(
                c => c.ComboId,
                c =>
                {
                    if (c.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho == 0))
                        return "Hết hàng";
                    if (c.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho < lowStockThreshold))
                        return "Sắp hết hàng";
                    return "Còn hàng";
                });

            var viewModel = new HomeViewModel
            {
                MonAns = monAns,
                Combos = combos,
                MonAnTrangThai = monAnTrangThai,
                ComboTrangThai = comboTrangThai
            };

            return View("Index", viewModel);
        }

        public IActionResult Detail(int id, string type)
        {
            const int lowStockThreshold = 10;

            if (string.IsNullOrEmpty(type))
            {
                return BadRequest("Loại không hợp lệ.");
            }

            if (type == "monan")
            {
                var monAn = _context.MonAns
                    .Include(m => m.LoaiMonAn)
                    .FirstOrDefault(m => m.MonAnId == id);
                if (monAn == null)
                {
                    return NotFound($"Không tìm thấy món ăn với ID {id}.");
                }
                ViewBag.TrangThai = monAn.SoLuongTonKho switch
                {
                    0 => "Hết hàng",
                    < lowStockThreshold => "Sắp hết hàng",
                    _ => "Còn hàng"
                };
                return View("Details", monAn);
            }
            else if (type == "combo")
            {
                var combo = _context.Combos
                    .Include(c => c.MonAnCombos)
                    .ThenInclude(mc => mc.MonAn)
                    .FirstOrDefault(c => c.ComboId == id);

                if (combo == null)
                {
                    return NotFound($"Không tìm thấy combo với ID {id}.");
                }
                ViewBag.TrangThai = combo.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho == 0) ? "Hết hàng"
                    : combo.MonAnCombos.Any(mc => mc.MonAn.SoLuongTonKho < lowStockThreshold) ? "Sắp hết hàng"
                    : "Còn hàng";

                // Tính trạng thái cho các món ăn trong combo
                ViewBag.MonAnTrangThai = combo.MonAnCombos.ToDictionary(
                    mc => mc.MonAnId,
                    mc => mc.MonAn.SoLuongTonKho switch
                    {
                        0 => "Hết hàng",
                        < lowStockThreshold => "Sắp hết hàng",
                        _ => "Còn hàng"
                    });

                return View("DetailCombo", combo);
            }

            return BadRequest("Loại yêu cầu không hợp lệ.");
        }
    }
}