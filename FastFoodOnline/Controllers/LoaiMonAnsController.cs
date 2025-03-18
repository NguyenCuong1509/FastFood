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
    public class LoaiMonAnsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoaiMonAnsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LoaiMonAns
        public async Task<IActionResult> Index()
        {
            return View(await _context.LoaiMonAns.ToListAsync());
        }

        // GET: LoaiMonAns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiMonAn = await _context.LoaiMonAns
                .FirstOrDefaultAsync(m => m.LoaiMonAnId == id);
            if (loaiMonAn == null)
            {
                return NotFound();
            }

            return View(loaiMonAn);
        }

        // GET: LoaiMonAns/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiMonAns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoaiMonAnId,TenLoai")] LoaiMonAn loaiMonAn)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiMonAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loaiMonAn);
        }

        // GET: LoaiMonAns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiMonAn = await _context.LoaiMonAns.FindAsync(id);
            if (loaiMonAn == null)
            {
                return NotFound();
            }
            return View(loaiMonAn);
        }

        // POST: LoaiMonAns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LoaiMonAnId,TenLoai")] LoaiMonAn loaiMonAn)
        {
            if (id != loaiMonAn.LoaiMonAnId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiMonAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiMonAnExists(loaiMonAn.LoaiMonAnId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loaiMonAn);
        }

        // GET: LoaiMonAns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiMonAn = await _context.LoaiMonAns
                .FirstOrDefaultAsync(m => m.LoaiMonAnId == id);
            if (loaiMonAn == null)
            {
                return NotFound();
            }

            return View(loaiMonAn);
        }

        // POST: LoaiMonAns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiMonAn = await _context.LoaiMonAns.FindAsync(id);
            if (loaiMonAn != null)
            {
                _context.LoaiMonAns.Remove(loaiMonAn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiMonAnExists(int id)
        {
            return _context.LoaiMonAns.Any(e => e.LoaiMonAnId == id);
        }
    }
}
