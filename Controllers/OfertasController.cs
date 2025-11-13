using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class OfertasController : Controller
    {
        private readonly ZonautoContext _context;

        public OfertasController(ZonautoContext context)
        {
            _context = context;
        }

        // ==============================
        // LISTA DE OFERTAS
        // ==============================
        public async Task<IActionResult> Index()
        {
            var ofertas = await _context.Ofertas
                .Include(o => o.Comprador)
                .Include(o => o.Publicacion)
                .AsNoTracking()
                .ToListAsync();

            return View(ofertas);
        }

        // ==============================
        // DETALLES DE UNA OFERTA
        // ==============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var oferta = await _context.Ofertas
                .Include(o => o.Comprador)
                .Include(o => o.Publicacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OfertaId == id);

            return oferta == null ? NotFound() : View(oferta);
        }

        // ==============================
        // REALIZAR OFERTA - GET
        // ==============================
        [HttpGet]
        public IActionResult RealizarOferta()
        {
            CargarSelectLists();
            return View();
        }

        // ==============================
        // REALIZAR OFERTA - POST
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarOferta(Oferta oferta)
        {
            if (oferta.Monto <= 0)
                ModelState.AddModelError("Monto", "El monto debe ser mayor que cero.");

            var publicacion = await _context.Publicaciones.FindAsync(oferta.PublicacionId);
            var comprador = await _context.Compradores.FindAsync(oferta.CompradorId);

            if (publicacion == null || comprador == null)
                ModelState.AddModelError("", "Publicación o comprador no encontrado.");

            if (!ModelState.IsValid)
            {
                CargarSelectLists(oferta.CompradorId, oferta.PublicacionId);
                return View(oferta);
            }

            oferta.FechaOferta = DateTime.Now;
            _context.Ofertas.Add(oferta);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Oferta realizada con éxito.";
            return RedirectToAction("Details", "Publicaciones", new { id = oferta.PublicacionId });
        }

        // ==============================
        // CANCELAR OFERTA - GET
        // ==============================
        [HttpGet]
        public async Task<IActionResult> CancelarOferta(int? id)
        {
            if (id == null) return NotFound();

            var oferta = await _context.Ofertas
                .Include(o => o.Comprador)
                .Include(o => o.Publicacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OfertaId == id);

            if (oferta == null)
                return NotFound();

            return View(oferta);
        }

        // ==============================
        // CANCELAR OFERTA - POST
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarOferta(int ofertaId, int compradorId)
        {
            var oferta = await _context.Ofertas
                .FirstOrDefaultAsync(o => o.OfertaId == ofertaId && o.CompradorId == compradorId);

            if (oferta == null)
            {
                TempData["Error"] = "No se encontró la oferta para cancelar.";
                return RedirectToAction(nameof(Index));
            }

            _context.Ofertas.Remove(oferta);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Oferta cancelada con éxito.";
            return RedirectToAction("Details", "Publicaciones", new { id = oferta.PublicacionId });
        }

        // ==============================
        // MÉTODOS AUXILIARES
        // ==============================
        private void CargarSelectLists(int? compradorId = null, int? publicacionId = null)
        {
            ViewBag.CompradorId = new SelectList(_context.Compradores, "CompradorId", "Nombre", compradorId);
            ViewBag.PublicacionId = new SelectList(_context.Publicaciones, "PublicacionId", "Titulo", publicacionId);
        }

        private bool OfertaExists(int id)
        {
            return _context.Ofertas.Any(e => e.OfertaId == id);
        }
    }
}
