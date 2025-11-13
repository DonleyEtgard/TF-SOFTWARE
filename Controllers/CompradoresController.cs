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
    public class CompradoresController : Controller
    {
        private readonly ZonautoContext _context;

        public CompradoresController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: Compradores
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.Compradores.Include(c => c.Usuario);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: Compradores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compradore = await _context.Compradores
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.CompradorId == id);
            if (compradore == null)
            {
                return NotFound();
            }

            return View(compradore);
        }

        // GET: Compradores/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: Compradores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompradorId,UsuarioId")] Compradore compradore)
        {
            if (ModelState.IsValid)
            {
                _context.Add(compradore);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", compradore.UsuarioId);
            return View(compradore);
        }

        // GET: Compradores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compradore = await _context.Compradores.FindAsync(id);
            if (compradore == null)
            {
                return NotFound();
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", compradore.UsuarioId);
            return View(compradore);
        }

        // POST: Compradores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompradorId,UsuarioId")] Compradore compradore)
        {
            if (id != compradore.CompradorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(compradore);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompradoreExists(compradore.CompradorId))
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
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", compradore.UsuarioId);
            return View(compradore);
        }

        [HttpGet]
        public IActionResult BuscarPublicaciones(string query)
        {
            var publicaciones = _context.Publicaciones
                .Include(p => p.Usuario)
                .Where(p => string.IsNullOrEmpty(query) ||
                            p.Titulo.Contains(query) ||
                            p.Descripcion.Contains(query))
                .ToList();

            return View("Publicaciones", publicaciones);
        }


    // GET: Compradores/Delete/5
    public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compradore = await _context.Compradores
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.CompradorId == id);
            if (compradore == null)
            {
                return NotFound();
            }

            return View(compradore);
        }

        // POST: Compradores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compradore = await _context.Compradores.FindAsync(id);
            if (compradore != null)
            {
                _context.Compradores.Remove(compradore);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompradoreExists(int id)
        {
            return _context.Compradores.Any(e => e.CompradorId == id);
        }
    }
}
