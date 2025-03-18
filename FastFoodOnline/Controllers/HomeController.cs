using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            int pageSize = 6; // Số lượng item trên mỗi trang

            var monAnList = await _context.MonAns
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var comboList = await _context.Combos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalMonAn = await _context.MonAns.CountAsync();
            int totalCombos = await _context.Combos.CountAsync();

            var viewModel = new HomeViewModel
            {
                MonAns = monAnList,
                Combos = comboList,
                CurrentPage = page,
                TotalPages = (int)System.Math.Ceiling((double)totalMonAn / pageSize),
                TotalPagesCombos = (int)System.Math.Ceiling((double)totalCombos / pageSize)
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MonAnPartial", viewModel);
            }

            return View(viewModel);
        }

public IActionResult Search(string keyword)
        {
            var viewModel = new HomeViewModel
            {
                MonAns = _context.MonAns
                    .Where(m => m.TenMon.Contains(keyword))
                    .ToList(),
                Combos = _context.Combos
                    .Where(c => c.TenCombo.Contains(keyword))
                    .ToList()
            };

            return View("Index", viewModel);
        }

        public IActionResult Detail(int id, string type)
        {
            if (type == "monan")
            {
                var monAn = _context.MonAns.FirstOrDefault(m => m.MonAnId == id);
                if (monAn == null) return NotFound();
                return View("Details", monAn);
            }
            else if (type == "combo")
            {
                var combo = _context.Combos
                    .Include(c => c.MonAnCombos) // Load danh sách món ăn trong combo
                    .ThenInclude(mc => mc.MonAn) // Load thông tin chi tiết món ăn
                    .FirstOrDefault(c => c.ComboId == id);

                if (combo == null) return NotFound();
                return View("DetailCombo", combo);
            }
            return NotFound();
        }

    }
}
