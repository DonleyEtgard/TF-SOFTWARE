using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class FavoritoesController : Controller
    {
        private readonly ZonautoContext _context;

        public FavoritoesController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: Favoritoes
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.Favoritos.Include(f => f.Comprador).Include(f => f.Publicacion);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: Favoritoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorito = await _context.Favoritos
                .Include(f => f.Comprador)
                .Include(f => f.Publicacion)
                .FirstOrDefaultAsync(m => m.FavoritoId == id);
            if (favorito == null)
            {
                return NotFound();
            }

            return View(favorito);
        }

        // GET: Favoritoes/Create
        public IActionResult Create()
        {
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId");
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId");
            return View();
        }

        // POST: Favoritoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FavoritoId,CompradorId,PublicacionId,FechaAgregado")] Favorito favorito)
        {
            if (ModelState.IsValid)
            {
                _context.Add(favorito);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId", favorito.CompradorId);
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", favorito.PublicacionId);
            return View(favorito);
        }

        // GET: Favoritoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorito = await _context.Favoritos.FindAsync(id);
            if (favorito == null)
            {
                return NotFound();
            }
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId", favorito.CompradorId);
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", favorito.PublicacionId);
            return View(favorito);
        }

        // POST: Favoritoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FavoritoId,CompradorId,PublicacionId,FechaAgregado")] Favorito favorito)
        {
            if (id != favorito.FavoritoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(favorito);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FavoritoExists(favorito.FavoritoId))
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
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId", favorito.CompradorId);
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", favorito.PublicacionId);
            return View(favorito);
        }

        // GET: Favoritoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorito = await _context.Favoritos
                .Include(f => f.Comprador)
                .Include(f => f.Publicacion)
                .FirstOrDefaultAsync(m => m.FavoritoId == id);
            if (favorito == null)
            {
                return NotFound();
            }

            return View(favorito);
        }

        // POST: Favoritoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var favorito = await _context.Favoritos.FindAsync(id);
            if (favorito != null)
            {
                _context.Favoritos.Remove(favorito);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FavoritoExists(int id)
        {
            return _context.Favoritos.Any(e => e.FavoritoId == id);
        }
    }
}
