using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FavoriteMovie.Data;
using FavoriteMovie.Models;

namespace FavoriteMovie.Controllers
{
    public class MediaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MediaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Media
        public async Task<IActionResult> Index()
        {
              return _context.Media != null ? 
                          View(await _context.Media.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Media'  is null.");
        }

        // GET: Media/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .FirstOrDefaultAsync(m => m.Id == id);
            if (media == null)
            {
                return NotFound();
            }

            return View(media);
        }

        // GET: Media/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Media/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Note,CreatedAt,UpdatedAt,AllocineDescription,AllocineLink,StreamingLink")] Media media)
        {
            if (ModelState.IsValid)
            {
                _context.Add(media);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(media);
        }

        // GET: Media/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            var media = await _context.Media.FindAsync(id);
            if (media == null)
            {
                return NotFound();
            }
            return View(media);
        }

        // POST: Media/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Note,CreatedAt,UpdatedAt,AllocineDescription,AllocineLink,StreamingLink")] Media media)
        {
            if (id != media.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(media);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MediaExists(media.Id))
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
            return View(media);
        }

        // GET: Media/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .FirstOrDefaultAsync(m => m.Id == id);
            if (media == null)
            {
                return NotFound();
            }

            return View(media);
        }

        // POST: Media/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Media == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Media'  is null.");
            }
            var media = await _context.Media.FindAsync(id);
            if (media != null)
            {
                _context.Media.Remove(media);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MediaExists(int id)
        {
          return (_context.Media?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
