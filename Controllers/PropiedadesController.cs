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
    public class PropiedadesController : Controller
    {
        private readonly ZonautoContext _context;

        public PropiedadesController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: Propriedades
        public async Task<IActionResult> Index()
        {
            return View(await _context.Propiedades.ToListAsync());
        }

        // GET: Propriedades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propriedade = await _context.Propiedades
                .FirstOrDefaultAsync(m => m.PropiedadId == id);
            if (propriedade == null)
            {
                return NotFound();
            }

            return View(propriedade);
        }

        // GET: Propriedades/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propriedades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PropiedadId,Ubicacion,MetroCuadrados,Habitaciones,Tipo,Descripcion")] Propiedade propiedade)
        {
            if (ModelState.IsValid)
            {
                _context.Add(propiedade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(propiedade);
        }

        // GET: Propriedades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propriedade = await _context.Propiedades.FindAsync(id);
            if (propriedade == null)
            {
                return NotFound();
            }
            return View(propriedade);
        }

        // POST: Propriedades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PropiedadId,Ubicacion,MetroCuadrados,Habitaciones,Tipo,Descripcion")] Propiedade propiedade)
        {
            if (id != propiedade.PropiedadId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(propiedade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropriedadeExists(propiedade.PropiedadId))
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
            return View(propiedade);
        }

        // GET: Propriedades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propiedade = await _context.Propiedades
                .FirstOrDefaultAsync(m => m.PropiedadId == id);
            if (propiedade == null)
            {
                return NotFound();
            }

            return View(propiedade);
        }

        // POST: Propriedades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propiedade = await _context.Propiedades.FindAsync(id);
            if (propiedade != null)
            {
                _context.Propiedades.Remove(propiedade);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropriedadeExists(int id)
        {
            return _context.Propiedades.Any(e => e.PropiedadId == id);
        }
    }
}
