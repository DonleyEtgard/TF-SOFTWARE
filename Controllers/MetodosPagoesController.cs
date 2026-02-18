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
    public class MetodosPagoesController : Controller
    {
        private readonly ZonautoContext _context;

        public MetodosPagoesController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: MetodosPagoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.MetodosPagos.ToListAsync());
        }

        // GET: MetodosPagoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metodosPago = await _context.MetodosPagos
                .FirstOrDefaultAsync(m => m.MetodoPagoId == id);
            if (metodosPago == null)
            {
                return NotFound();
            }

            return View(metodosPago);
        }

        // GET: MetodosPagoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MetodosPagoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MetodoPagoId,TipoDePago,Descripcion")] MetodosPago metodosPago)
        {
            if (ModelState.IsValid)
            {
                _context.Add(metodosPago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(metodosPago);
        }

        // GET: MetodosPagoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metodosPago = await _context.MetodosPagos.FindAsync(id);
            if (metodosPago == null)
            {
                return NotFound();
            }
            return View(metodosPago);
        }

        // POST: MetodosPagoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MetodoPagoId,TipoDePago,Descripcion")] MetodosPago metodosPago)
        {
            if (id != metodosPago.MetodoPagoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(metodosPago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MetodosPagoExists(metodosPago.MetodoPagoId))
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
            return View(metodosPago);
        }

        // GET: MetodosPagoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metodosPago = await _context.MetodosPagos
                .FirstOrDefaultAsync(m => m.MetodoPagoId == id);
            if (metodosPago == null)
            {
                return NotFound();
            }

            return View(metodosPago);
        }

        // POST: MetodosPagoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var metodosPago = await _context.MetodosPagos.FindAsync(id);
            if (metodosPago != null)
            {
                _context.MetodosPagos.Remove(metodosPago);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MetodosPagoExists(int id)
        {
            return _context.MetodosPagos.Any(e => e.MetodoPagoId == id);
        }
    }
}
