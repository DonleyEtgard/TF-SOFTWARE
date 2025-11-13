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
    public class VentasController : Controller
    {
        private readonly ZonautoContext _context;

        public VentasController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.Ventas.Include(v => v.Comprador).Include(v => v.Oferta).Include(v => v.Publicacion).Include(v => v.Transaccion).Include(v => v.Vendedor);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Comprador)
                .Include(v => v.Oferta)
                .Include(v => v.Publicacion)
                .Include(v => v.Transaccion)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(m => m.VentaId == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId");
            ViewData["OfertaId"] = new SelectList(_context.Ofertas, "OfertaId", "OfertaId");
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId");
            ViewData["TransaccionId"] = new SelectList(_context.Transacciones, "TransaccionId", "TransaccionId");
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId");
            return View();
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VentaId,OfertaId,VendedorId,CompradorId,PublicacionId,TransaccionId,PrecioFinal,FechaVenta,EstadoVenta")] Venta venta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId", venta.CompradorId);
            ViewData["OfertaId"] = new SelectList(_context.Ofertas, "OfertaId", "OfertaId", venta.OfertaId);
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", venta.PublicacionId);
            ViewData["TransaccionId"] = new SelectList(_context.Transacciones, "TransaccionId", "TransaccionId", venta.TransaccionId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId", venta.VendedorId);
            return View(venta);
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId", venta.CompradorId);
            ViewData["OfertaId"] = new SelectList(_context.Ofertas, "OfertaId", "OfertaId", venta.OfertaId);
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", venta.PublicacionId);
            ViewData["TransaccionId"] = new SelectList(_context.Transacciones, "TransaccionId", "TransaccionId", venta.TransaccionId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId", venta.VendedorId);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VentaId,OfertaId,VendedorId,CompradorId,PublicacionId,TransaccionId,PrecioFinal,FechaVenta,EstadoVenta")] Venta venta)
        {
            if (id != venta.VentaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.VentaId))
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
            ViewData["CompradorId"] = new SelectList(_context.Compradores, "CompradorId", "CompradorId", venta.CompradorId);
            ViewData["OfertaId"] = new SelectList(_context.Ofertas, "OfertaId", "OfertaId", venta.OfertaId);
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", venta.PublicacionId);
            ViewData["TransaccionId"] = new SelectList(_context.Transacciones, "TransaccionId", "TransaccionId", venta.TransaccionId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores, "VendedorId", "VendedorId", venta.VendedorId);
            return View(venta);
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Comprador)
                .Include(v => v.Oferta)
                .Include(v => v.Publicacion)
                .Include(v => v.Transaccion)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(m => m.VentaId == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                _context.Ventas.Remove(venta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.VentaId == id);
        }
    }
}
