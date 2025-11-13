using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ZonautoContext _context;

        public TransaccionesController(ZonautoContext context)
        {
            _context = context;
        }

        // =========================
        // LISTADO GENERAL
        // =========================
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.Transacciones
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Publicacion);

            return View(await zonautoContext.ToListAsync());
        }

        // =========================
        // DETALLES
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var transaccion = await _context.Transacciones
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Publicacion)
                .FirstOrDefaultAsync(m => m.TransaccionId == id);

            if (transaccion == null) return NotFound();

            return View(transaccion);
        }

        // =========================
        // CONFIRMAR PAGO (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> ConfirmarPago(int id)
        {
            var transaccion = await _context.Transacciones
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Publicacion)
                .FirstOrDefaultAsync(t => t.TransaccionId == id);

            if (transaccion == null) return NotFound();

            // Lista de métodos de pago
            ViewBag.MetodosPago = new SelectList(_context.MetodosPagos, "MetodoPagoId", "TipoDePago");

            return View(transaccion);
        }

        // =========================
        // CONFIRMAR PAGO (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPago(int id, int metodoPagoId)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null) return NotFound();

            // Validar que el método de pago exista
            var metodo = await _context.MetodosPagos.FindAsync(metodoPagoId);
            if (metodo == null)
            {
                TempData["Error"] = "Método de pago inválido.";
                return RedirectToAction(nameof(ConfirmarPago), new { id });
            }

            // Confirmar pago
            transaccion.MetodoDePago = metodoPagoId.ToString(); // Guarda ID como string
            transaccion.Confirmado = true;
            transaccion.Fecha = DateTime.Now;

            _context.Update(transaccion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pago confirmado exitosamente.";

            return RedirectToAction(nameof(ConfirmacionTransaccion), new { id = transaccion.TransaccionId });
        }

        // =========================
        // CONFIRMACIÓN FINAL DE LA TRANSACCIÓN
        // =========================
        [HttpGet]
        public async Task<IActionResult> ConfirmacionTransaccion(int id)
        {
            var transaccion = await _context.Transacciones
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Publicacion)
                .FirstOrDefaultAsync(t => t.TransaccionId == id);

            if (transaccion == null) return NotFound();

            // Obtener el nombre del método de pago
            string metodoPagoNombre = "No definido";
            if (!string.IsNullOrEmpty(transaccion.MetodoDePago)
                && int.TryParse(transaccion.MetodoDePago, out int metodoId))
            {
                var metodo = await _context.MetodosPagos.FindAsync(metodoId);
                if (metodo != null) metodoPagoNombre = metodo.TipoDePago;
            }
            else if (!string.IsNullOrEmpty(transaccion.MetodoDePago))
            {
                metodoPagoNombre = transaccion.MetodoDePago;
            }

            ViewBag.MetodoPagoNombre = metodoPagoNombre;

            return View(transaccion);
        }

        // =========================
        // CREATE
        // =========================
        public IActionResult Create()
        {
            ViewData["CompradorId"] = new SelectList(_context.Compradores
                .Include(c => c.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "CompradorId", "Usuario.Persona.Nombre");

            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "Titulo");

            ViewData["VendedorId"] = new SelectList(_context.Vendedores
                .Include(v => v.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "VendedorId", "Usuario.Persona.Nombre");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransaccionId,CompradorId,VendedorId,PublicacionId,Fecha,MontoFinal,MetodoDePago,Confirmado")] Transaccione transaccione)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaccione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompradorId"] = new SelectList(_context.Compradores
                .Include(c => c.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "CompradorId", "Usuario.Persona.Nombre", transaccione.CompradorId);

            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "Titulo", transaccione.PublicacionId);

            ViewData["VendedorId"] = new SelectList(_context.Vendedores
                .Include(v => v.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "VendedorId", "Usuario.Persona.Nombre", transaccione.VendedorId);

            return View(transaccione);
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaccione = await _context.Transacciones.FindAsync(id);
            if (transaccione == null) return NotFound();

            ViewData["CompradorId"] = new SelectList(_context.Compradores
                .Include(c => c.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "CompradorId", "Usuario.Persona.Nombre", transaccione.CompradorId);

            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "Titulo", transaccione.PublicacionId);

            ViewData["VendedorId"] = new SelectList(_context.Vendedores
                .Include(v => v.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "VendedorId", "Usuario.Persona.Nombre", transaccione.VendedorId);

            return View(transaccione);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransaccionId,CompradorId,VendedorId,PublicacionId,Fecha,MontoFinal,MetodoDePago,Confirmado")] Transaccione transaccione)
        {
            if (id != transaccione.TransaccionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaccione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransaccioneExists(transaccione.TransaccionId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompradorId"] = new SelectList(_context.Compradores
                .Include(c => c.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "CompradorId", "Usuario.Persona.Nombre", transaccione.CompradorId);

            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "Titulo", transaccione.PublicacionId);

            ViewData["VendedorId"] = new SelectList(_context.Vendedores
                .Include(v => v.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToList(), "VendedorId", "Usuario.Persona.Nombre", transaccione.VendedorId);

            return View(transaccione);
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var transaccione = await _context.Transacciones
                .Include(t => t.Comprador)
                    .ThenInclude(c => c.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Vendedor)
                    .ThenInclude(v => v.Usuario)
                        .ThenInclude(u => u.Persona)
                .Include(t => t.Publicacion)
                .FirstOrDefaultAsync(m => m.TransaccionId == id);

            if (transaccione == null) return NotFound();

            return View(transaccione);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaccione = await _context.Transacciones.FindAsync(id);
            if (transaccione != null)
            {
                _context.Transacciones.Remove(transaccione);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // AUXILIAR
        // =========================
        private bool TransaccioneExists(int id)
        {
            return _context.Transacciones.Any(e => e.TransaccionId == id);
        }
    }
}

