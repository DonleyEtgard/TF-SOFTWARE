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
    public class DireccionesController : Controller
    {
        private readonly ZonautoContext _context;

        public DireccionesController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: Direcciones
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.Direcciones.Include(d => d.Persona);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: Direcciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var direccione = await _context.Direcciones
                .Include(d => d.Persona)
                .FirstOrDefaultAsync(m => m.DireccionId == id);
            if (direccione == null)
            {
                return NotFound();
            }

            return View(direccione);
        }

        // GET: Direcciones/Create
        public IActionResult Create()
        {
            ViewData["PersonaId"] = new SelectList(_context.Personas, "PersonaId", "PersonaId");
            return View();
        }

        // POST: Direcciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DireccionId,PersonaId,Calle,Ciudad,Provincia,CodigoPostal,Pais")] Direccione direccione)
        {
            if (ModelState.IsValid)
            {
                _context.Add(direccione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PersonaId"] = new SelectList(_context.Personas, "PersonaId", "PersonaId", direccione.PersonaId);
            return View(direccione);
        }

        // GET: Direcciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var direccione = await _context.Direcciones.FindAsync(id);
            if (direccione == null)
            {
                return NotFound();
            }
            ViewData["PersonaId"] = new SelectList(_context.Personas, "PersonaId", "PersonaId", direccione.PersonaId);
            return View(direccione);
        }

        // POST: Direcciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DireccionId,PersonaId,Calle,Ciudad,Provincia,CodigoPostal,Pais")] Direccione direccione)
        {
            if (id != direccione.DireccionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(direccione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DireccioneExists(direccione.DireccionId))
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
            ViewData["PersonaId"] = new SelectList(_context.Personas, "PersonaId", "PersonaId", direccione.PersonaId);
            return View(direccione);
        }

        // GET: Direcciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var direccione = await _context.Direcciones
                .Include(d => d.Persona)
                .FirstOrDefaultAsync(m => m.DireccionId == id);
            if (direccione == null)
            {
                return NotFound();
            }

            return View(direccione);
        }

        // POST: Direcciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var direccione = await _context.Direcciones.FindAsync(id);
            if (direccione != null)
            {
                _context.Direcciones.Remove(direccione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DireccioneExists(int id)
        {
            return _context.Direcciones.Any(e => e.DireccionId == id);
        }
    }
}
