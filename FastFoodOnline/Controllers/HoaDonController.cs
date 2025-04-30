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
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HoaDon
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.HoaDons.Include(h => h.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: HoaDon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(hdct => hdct.MonAn)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(hdct => hdct.Combo)
                        .ThenInclude(c => c.MonAnCombos)
                            .ThenInclude(mc => mc.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonId == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Đảm bảo danh sách không bị null
            hoaDon.HoaDonChiTiets ??= new List<HoaDonChiTiet>();

            return View(hoaDon);
        }

        // POST: HoaDon/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, TrangThaiHoaDon trangThai)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(hdct => hdct.MonAn)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(hdct => hdct.Combo)
                        .ThenInclude(c => c.MonAnCombos)
                            .ThenInclude(mc => mc.MonAn)
                .FirstOrDefaultAsync(h => h.HoaDonId == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Cho phép hủy đơn hàng bất kỳ lúc nào
            if (trangThai == TrangThaiHoaDon.DaHuy)
            {
                // Chỉ hủy nếu chưa hủy trước đó để tránh hoàn tồn kho nhiều lần
                if (hoaDon.TrangThai != TrangThaiHoaDon.DaHuy)
                {
                    hoaDon.TrangThai = TrangThaiHoaDon.DaHuy;

                    // Hoàn lại số lượng tồn kho
                    foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                    {
                        // Hoàn tồn kho cho món ăn
                        if (chiTiet.MonAnId.HasValue && chiTiet.MonAn != null)
                        {
                            chiTiet.MonAn.SoLuongTonKho += chiTiet.SoLuong;
                        }

                        // Hoàn tồn kho cho các món ăn trong combo
                        if (chiTiet.ComboId.HasValue && chiTiet.Combo != null)
                        {
                            foreach (var monAnCombo in chiTiet.Combo.MonAnCombos)
                            {
                                if (monAnCombo.MonAn != null)
                                {
                                    monAnCombo.MonAn.SoLuongTonKho += chiTiet.SoLuong * monAnCombo.SoLuong;
                                }
                            }
                        }
                    }
                }
            }
            // Chỉ cho phép cập nhật sang trạng thái tiếp theo
            else if ((int)trangThai == (int)hoaDon.TrangThai + 1)
            {
                hoaDon.TrangThai = trangThai;
            }
            else
            {
                ModelState.AddModelError("", "Bạn chỉ có thể cập nhật sang trạng thái tiếp theo hoặc hủy đơn.");
                return RedirectToAction(nameof(Details), new { id });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}