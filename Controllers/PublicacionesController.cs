using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;
namespace ZONAUTO.Controllers
{
    public class PublicacionesController : Controller
    {
        private readonly ZonautoContext _context;

        public PublicacionesController(ZonautoContext context)
        {
            _context = context;
        }
        
        // GET: Publicaciones
        public async Task<IActionResult> Index()
        {
            int usuarioId = GetUsuarioIdAutenticado();

            var publicaciones = await _context.Publicaciones
                   .Where(p => p.UsuarioId == usuarioId)
                   .Include(p => p.Categoria)
                   .Include(p => p.Usuario)
                       .ThenInclude(u => u.Persona)
                   .Include(p => p.Auto)
                   .Include(p => p.Propiedad)
                   .ToListAsync();

           // 🔹 Usamos Usuario en lugar de Vendedor
            return View(publicaciones);
        }

        // GET: Publicaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var publicacion = await _context.Publicaciones
                .Include(p => p.Categoria)
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona) // Datos del vendedor
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null) return NotFound();

            return View(publicacion);
        }

        // GET: Publicaciones/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Usuarios, "UsuarioId", "Nombre");
            return View();
        }

        // POST: Publicaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PublicacionId,Titulo,Descripcion,FechaPublicacion,UsuarioId")] Publicacione publicacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publicacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Usuarios, "UsuarioId", "Nombre", publicacion.UsuarioId);
            return View(publicacion);
        }

        // GET: Publicaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            ViewData["UsuarioId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Usuarios, "UsuarioId", "Nombre", publicacion.UsuarioId);
            return View(publicacion);
        }

        // POST: Publicaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PublicacionId,Titulo,Descripcion,FechaPublicacion,UsuarioId")] Publicacione publicacion)
        {
            if (id != publicacion.PublicacionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publicacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicacionExists(publicacion.PublicacionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Usuarios, "UsuarioId", "Nombre", publicacion.UsuarioId);
            return View(publicacion);
        }

        // GET: Publicaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var publicacion = await _context.Publicaciones
                .Include(p => p.Usuario) // 🔹 Relación correcta
                .FirstOrDefaultAsync(m => m.PublicacionId == id);

            if (publicacion == null) return NotFound();

            return View(publicacion);
        }

        // POST: Publicaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion != null)
            {
                _context.Publicaciones.Remove(publicacion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PublicacionExists(int id)
        {
            return _context.Publicaciones.Any(e => e.PublicacionId == id);
        }
        // ===================== Obtener Vendedor autenticado =====================
        private int GetVendedorIdAutenticado()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "UsuarioId");

            if (claim != null && int.TryParse(claim.Value, out int usuarioId))
            {
                var vendedor = _context.Vendedores.FirstOrDefault(v => v.UsuarioId == usuarioId);
                return vendedor?.VendedorId ?? 0;
            }

            return 0;
        }

        // ===================== Obtener Usuario autenticado =====================
        private int GetUsuarioIdAutenticado()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "UsuarioId");
            return claim != null && int.TryParse(claim.Value, out int usuarioId) ? usuarioId : 0;
        }
    }
}