using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FastFoodOnline.Controllers
{
    public class MonAnController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonAnController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách món ăn
        public async Task<IActionResult> Index()
        {
            var monAnList = await _context.MonAns.Include(m => m.LoaiMonAn).ToListAsync();
            return View(monAnList);
        }

        // Chi tiết món ăn
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns.Include(m => m.LoaiMonAn)
                                            .FirstOrDefaultAsync(m => m.MonAnId == id);
            if (monAn == null) return NotFound();

            return View(monAn);
        }

        // GET: Tạo món ăn
        public IActionResult Create()
        {
            var loaiMonAnList = _context.LoaiMonAns?.ToList();
            if (loaiMonAnList == null || !loaiMonAnList.Any())
            {
                ModelState.AddModelError("", "Không có loại món ăn nào, vui lòng thêm trước!");
            }

            ViewBag.LoaiMonAnId = new SelectList(loaiMonAnList, "LoaiMonAnId", "TenLoai");
            return View();
        }

        // POST: Tạo món ăn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenMon,Gia,MoTa,HinhAnh,LoaiMonAnId,SoLuongTonKho")] MonAn monAn)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Ghi log lỗi ra console
                }

                ViewBag.LoaiMonAnId = new SelectList(_context.LoaiMonAns, "LoaiMonAnId", "TenLoai");
                return View(monAn);
            }

            try
            {
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu món ăn: " + ex.Message);
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu món ăn. Vui lòng thử lại!");
                ViewBag.LoaiMonAnId = new SelectList(_context.LoaiMonAns, "LoaiMonAnId", "TenLoai");
                return View(monAn);
            }
        }

        // GET: Sửa món ăn
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return NotFound();

            // Lưu trạng thái ban đầu vào session để có thể rollback
            HttpContext.Session.SetString("BackupMonAn", JsonConvert.SerializeObject(monAn));

            ViewBag.LoaiMonAnId = new SelectList(_context.LoaiMonAns, "LoaiMonAnId", "TenLoai", monAn.LoaiMonAnId);
            return View(monAn);
        }

        // POST: Sửa món ăn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MonAnId,TenMon,Gia,MoTa,HinhAnh,LoaiMonAnId,SoLuongTonKho")] MonAn monAn)
        {
            if (id != monAn.MonAnId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.LoaiMonAnId = new SelectList(_context.LoaiMonAns, "LoaiMonAnId", "TenLoai", monAn.LoaiMonAnId);
                return View(monAn);
            }

            try
            {
                _context.Update(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.MonAns.Any(m => m.MonAnId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi sửa món ăn: " + ex.Message);
                ModelState.AddModelError("", "Có lỗi xảy ra khi sửa món ăn. Vui lòng thử lại!");
                ViewBag.LoaiMonAnId = new SelectList(_context.LoaiMonAns, "LoaiMonAnId", "TenLoai", monAn.LoaiMonAnId);
                return View(monAn);
            }
        }

        // Rollback sửa món ăn
        public async Task<IActionResult> RollbackEdit(int id)
        {
            var monAnJson = HttpContext.Session.GetString("BackupMonAn");
            if (string.IsNullOrEmpty(monAnJson))
            {
                TempData["Error"] = "Không có dữ liệu cũ để khôi phục!";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var monAnBackup = JsonConvert.DeserializeObject<MonAn>(monAnJson);
            if (monAnBackup == null || monAnBackup.MonAnId != id)
            {
                TempData["Error"] = "Lỗi khi khôi phục dữ liệu!";
                return RedirectToAction(nameof(Edit), new { id });
            }

            try
            {
                _context.Entry(monAnBackup).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Dữ liệu đã được khôi phục!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi khôi phục món ăn: " + ex.Message);
                TempData["Error"] = "Có lỗi xảy ra khi khôi phục dữ liệu!";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }
    }
}