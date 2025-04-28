using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFoodOnline.Data;
using FastFoodOnline.Models;

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

            try
            {
                _context.Combos.Add(combo);
                await _context.SaveChangesAsync();

                for (int i = 0; i < monAnIds.Count; i++)
                {
                    if (soLuongs[i] <= 0) continue; // Bỏ qua nếu số lượng không hợp lệ
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
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo combo: " + ex.Message);
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

            try
            {
                var existingCombo = await _context.Combos
                    .Include(c => c.MonAnCombos)
                    .FirstOrDefaultAsync(c => c.ComboId == id);

                if (existingCombo == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin combo
                existingCombo.TenCombo = combo.TenCombo;
                existingCombo.GiaGoc = combo.GiaGoc;
                existingCombo.GiaKhuyenMai = combo.GiaKhuyenMai;
                existingCombo.HinhAnh = combo.HinhAnh; // Cập nhật URL hình ảnh

                // Xóa món ăn cũ
                _context.MonAnCombos.RemoveRange(existingCombo.MonAnCombos);

                // Thêm món ăn mới
                for (int i = 0; i < monAnIds.Count; i++)
                {
                    if (soLuongs[i] <= 0) continue;
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
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật combo: " + ex.Message);
                ViewBag.MonAnList = _context.MonAns.ToList();
                return View(combo);
            }
        }
    }
}