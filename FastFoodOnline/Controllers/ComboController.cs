using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Index()
        {
            var combos = _context.Combos.Include(c => c.MonAnCombos).ThenInclude(mc => mc.MonAn).ToList();
            return View(combos);
        }

        // Form tạo combo
        public IActionResult Create()
        {
            ViewBag.MonAnList = _context.MonAns.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Combo combo, List<int> monAnIds, List<int> soLuongs)
        {
            if (ModelState.IsValid)
            {
                _context.Combos.Add(combo);
                _context.SaveChanges();

                for (int i = 0; i < monAnIds.Count; i++)
                {
                    _context.MonAnCombos.Add(new MonAnCombo
                    {
                        ComboId = combo.ComboId,
                        MonAnId = monAnIds[i],
                        SoLuong = soLuongs[i]
                    });
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MonAnList = _context.MonAns.ToList();
            return View(combo);
        }
        // Form chỉnh sửa Combo
        public IActionResult Edit(int id)
        {
            var combo = _context.Combos
                .Include(c => c.MonAnCombos)
                .FirstOrDefault(c => c.ComboId == id);

            if (combo == null)
            {
                return NotFound();
            }

            ViewBag.MonAnList = _context.MonAns.ToList();
            return View(combo);
        }

        [HttpPost]
        public IActionResult Edit(int id, Combo combo, List<int> monAnIds, List<int> soLuongs)
        {
            if (id != combo.ComboId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingCombo = _context.Combos
                    .Include(c => c.MonAnCombos)
                    .FirstOrDefault(c => c.ComboId == id);

                if (existingCombo == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin Combo
                existingCombo.TenCombo = combo.TenCombo;
                existingCombo.GiaGoc = combo.GiaGoc;
                existingCombo.GiaKhuyenMai = combo.GiaKhuyenMai;

                // Xóa các món ăn cũ
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

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MonAnList = _context.MonAns.ToList();
            return View(combo);
        }

    }
}
