using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastFoodOnline.Controllers
{
    public class ComboController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComboController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách combo
        public async Task<IActionResult> Index()
        {
            var combos = await _context.Combos
                .Include(c => c.MonAnCombos)
                .ThenInclude(mc => mc.MonAn)
                .ToListAsync();
            return View(combos);
        }

        // Form tạo combo
        public IActionResult Create()
        {
            ViewBag.MonAnList = _context.MonAns.ToList();
            return View();
        }

        // POST: Tạo combo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Combo combo, List<int> monAnIds, List<int> soLuongs)
        {
            if (!ModelState.IsValid || monAnIds.Count != soLuongs.Count || monAnIds.Count == 0)
            {
                ViewBag.MonAnList = _context.MonAns.ToList();
                ModelState.AddModelError("", "Dữ liệu không hợp lệ hoặc danh sách món ăn trống.");
                return View(combo);
            }

            // Kiểm tra giá khuyến mãi
            if (combo.GiaKhuyenMai >= combo.GiaGoc)
            {
                ModelState.AddModelError("GiaKhuyenMai", "Giá khuyến mãi phải nhỏ hơn giá gốc.");
                ViewBag.MonAnList = _context.MonAns.ToList();
                return View(combo);
            }

            try
            {
                // Kiểm tra tồn kho
                var monAns = await _context.MonAns.Where(m => monAnIds.Contains(m.MonAnId)).ToListAsync();
                for (int i = 0; i < monAnIds.Count; i++)
                {
                    if (soLuongs[i] <= 0)
                    {
                        ModelState.AddModelError("", $"Số lượng món ăn ID {monAnIds[i]} phải lớn hơn 0.");
                        ViewBag.MonAnList = _context.MonAns.ToList();
                        return View(combo);
                    }
                    var monAn = monAns.FirstOrDefault(m => m.MonAnId == monAnIds[i]);
                    if (monAn == null || monAn.SoLuongTonKho < soLuongs[i])
                    {
                        ModelState.AddModelError("", $"Món ăn {monAn?.TenMon ?? "ID " + monAnIds[i]} không đủ số lượng trong kho (còn {monAn?.SoLuongTonKho ?? 0}).");
                        ViewBag.MonAnList = _context.MonAns.ToList();
                        return View(combo);
                    }
                }

                // Tạo combo
                _context.Combos.Add(combo);
                await _context.SaveChangesAsync();

                // Thêm món ăn vào combo
                for (int i = 0; i < monAnIds.Count; i++)
                {
                    _context.MonAnCombos.Add(new MonAnCombo
                    {
                        ComboId = combo.ComboId,
                        MonAnId = monAnIds[i],
                        SoLuong = soLuongs[i]
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo combo: {ex.Message}");
                ViewBag.MonAnList = _context.MonAns.ToList();
                return View(combo);
            }
        }

        // Form chỉnh sửa combo
        public async Task<IActionResult> Edit(int id)
        {
            var combo = await _context.Combos
                .Include(c => c.MonAnCombos)
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null)
            {
                return NotFound();
            }

            ViewBag.MonAnList = _context.MonAns.ToList();
            return View(combo);
        }

        // POST: Chỉnh sửa combo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Combo combo, List<int> monAnIds, List<int> soLuongs)
        {
            if (id != combo.ComboId || monAnIds.Count != soLuongs.Count || monAnIds.Count == 0)
            {
                ViewBag.MonAnList = _context.MonAns.ToList();
                ModelState.AddModelError("", "Dữ liệu không hợp lệ hoặc danh sách món ăn trống.");
                return View(combo);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MonAnList = _context.MonAns.ToList();
                return View(combo);
            }

            // Kiểm tra giá khuyến mãi
            if (combo.GiaKhuyenMai >= combo.GiaGoc)
            {
                ModelState.AddModelError("GiaKhuyenMai", "Giá khuyến mãi phải nhỏ hơn giá gốc.");
                ViewBag.MonAnList = _context.MonAns.ToList();
                return View(combo);
            }

            try
            {
                // Kiểm tra tồn kho
                var monAns = await _context.MonAns.Where(m => monAnIds.Contains(m.MonAnId)).ToListAsync();
                for (int i = 0; i < monAnIds.Count; i++)
                {
                    if (soLuongs[i] <= 0)
                    {
                        ModelState.AddModelError("", $"Số lượng món ăn ID {monAnIds[i]} phải lớn hơn 0.");
                        ViewBag.MonAnList = _context.MonAns.ToList();
                        return View(combo);
                    }
                    var monAn = monAns.FirstOrDefault(m => m.MonAnId == monAnIds[i]);
                    if (monAn == null || monAn.SoLuongTonKho < soLuongs[i])
                    {
                        ModelState.AddModelError("", $"Món ăn {monAn?.TenMon ?? "ID " + monAnIds[i]} không đủ số lượng trong kho (còn {monAn?.SoLuongTonKho ?? 0}).");
                        ViewBag.MonAnList = _context.MonAns.ToList();
                        return View(combo);
                    }
                }

                // Cập nhật combo
                var existingCombo = await _context.Combos
                    .Include(c => c.MonAnCombos)
                    .FirstOrDefaultAsync(c => c.ComboId == id);

                if (existingCombo == null)
                {
                    return NotFound();
                }

                existingCombo.TenCombo = combo.TenCombo;
                existingCombo.GiaGoc = combo.GiaGoc;
                existingCombo.GiaKhuyenMai = combo.GiaKhuyenMai;
                existingCombo.HinhAnh = combo.HinhAnh;

                // Xóa món ăn cũ
                _context.MonAnCombos.RemoveRange(existingCombo.MonAnCombos);

                // Thêm món ăn mới
                for (int i = 0; i < monAnIds.Count; i++)
                {
                    _context.MonAnCombos.Add(new MonAnCombo
                    {
                        ComboId = combo.ComboId,
                        MonAnId = monAnIds[i],
                        SoLuong = soLuongs[i]
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật combo: {ex.Message}");
                ViewBag.MonAnList = _context.MonAns.ToList();
                return View(combo);
            }
        }
    }
}