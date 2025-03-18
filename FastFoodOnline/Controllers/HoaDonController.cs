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
                .FirstOrDefaultAsync(h => h.HoaDonId == id);

            if (hoaDon == null)
            {
                return NotFound(); // Trả về trang lỗi nếu không tìm thấy
            }

            // Đảm bảo danh sách không bị null
            hoaDon.HoaDonChiTiets ??= new List<HoaDonChiTiet>();

            return View(hoaDon);
        }

        // POST: HoaDon/UpdateStatus
        // POST: HoaDon/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, TrangThaiHoaDon trangThai)
        {
            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            // Cho phép hủy đơn hàng bất kỳ lúc nào
            if (trangThai == TrangThaiHoaDon.DaHuy)
            {
                hoaDon.TrangThai = TrangThaiHoaDon.DaHuy;
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
