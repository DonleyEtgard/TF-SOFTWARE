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
    public class PublicacionesPremiumsController : Controller
    {
        private readonly ZonautoContext _context;

        public PublicacionesPremiumsController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: PublicacionesPremiums
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.PublicacionesPremia.Include(p => p.Publicacion).Include(p => p.Vendedor);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: PublicacionesPremiums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicacionesPremium = await _context.PublicacionesPremia
                .Include(p => p.Publicacion)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(m => m.PublicacionPremiumId == id);
            if (publicacionesPremium == null)
            {
                return NotFound();
            }

            return View(publicacionesPremium);
        }

        // GET: PublicacionesPremiums/Create
        public IActionResult Create()
        {
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId");
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId");
            return View();
        }

        // POST: PublicacionesPremiums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PublicacionPremiumId,PublicacionId,FechaInicio,FechaFin,Estado,VendedorId")] PublicacionesPremium publicacionesPremium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publicacionesPremium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", publicacionesPremium.PublicacionId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId", publicacionesPremium.VendedorId);
            return View(publicacionesPremium);
        }

        // GET: PublicacionesPremiums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicacionesPremium = await _context.PublicacionesPremia.FindAsync(id);
            if (publicacionesPremium == null)
            {
                return NotFound();
            }
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", publicacionesPremium.PublicacionId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId", publicacionesPremium.VendedorId);
            return View(publicacionesPremium);
        }

        // POST: PublicacionesPremiums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PublicacionPremiumId,PublicacionId,FechaInicio,FechaFin,Estado,VendedorId")] PublicacionesPremium publicacionesPremium)
        {
            if (id != publicacionesPremium.PublicacionPremiumId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publicacionesPremium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicacionesPremiumExists(publicacionesPremium.PublicacionPremiumId))
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
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", publicacionesPremium.PublicacionId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId", publicacionesPremium.VendedorId);
            return View(publicacionesPremium);
        }

        // GET: PublicacionesPremiums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publicacionesPremium = await _context.PublicacionesPremia
                .Include(p => p.Publicacion)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(m => m.PublicacionPremiumId == id);
            if (publicacionesPremium == null)
            {
                return NotFound();
            }

            return View(publicacionesPremium);
        }

        // POST: PublicacionesPremiums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publicacionesPremium = await _context.PublicacionesPremia.FindAsync(id);
            if (publicacionesPremium != null)
            {
                _context.PublicacionesPremia.Remove(publicacionesPremium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublicacionesPremiumExists(int id)
        {
            return _context.PublicacionesPremia.Any(e => e.PublicacionPremiumId == id);
        }
    }
}
