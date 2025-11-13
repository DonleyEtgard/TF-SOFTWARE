using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class VendedoresController : Controller
    {
        private readonly ZonautoContext _context;

        public VendedoresController(ZonautoContext context)
        {
            _context = context;
        }

        // ===================== INDEX =====================
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

                return View(publicaciones);
            
        }
    

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int id)
        {
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
        // ===================== DELETE =====================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Categoria)
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null) return NotFound();

            // Verificar que la publicación pertenece al vendedor autenticado
            int vendedorId = GetVendedorIdAutenticado();
            if (publicacion.VendedorId != vendedorId) return Unauthorized();

            return View(publicacion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();

            // Verificar que la publicación pertenece al vendedor autenticado
            int vendedorId = GetVendedorIdAutenticado();
            if (publicacion.VendedorId != vendedorId) return Unauthorized();

            // Eliminar la imagen física si existe
            if (!string.IsNullOrEmpty(publicacion.ImagenRuta))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", publicacion.ImagenRuta.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Publicaciones.Remove(publicacion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // ===================== CREAR PUBLICACION =====================

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
