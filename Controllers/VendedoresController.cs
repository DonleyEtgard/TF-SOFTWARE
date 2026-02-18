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
        [HttpGet]
        public IActionResult CrearPublicacion()
        {
            ViewBag.Categorias = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
            ViewBag.Autos = new SelectList(_context.Autos, "AutoId", "Modelo");
            ViewBag.Propiedades = new SelectList(_context.Propiedades, "PropiedadId", "Ubicacion");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPublicacion(
            Publicacione publicacion,
            int? autoSeleccionado,
            int? propiedadSeleccionada,
            IFormFile ImagenPrincipal)
        {
            // Validación: no permitir ambos seleccionados
            if (autoSeleccionado.HasValue && propiedadSeleccionada.HasValue)
            {
                ModelState.AddModelError("", "Debe seleccionar Auto o Propiedad, no ambos.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(_context.Categorias, "CategoriaId", "Nombre", publicacion.CategoriaId);
                ViewBag.Autos = new SelectList(_context.Autos, "AutoId", "Modelo", autoSeleccionado);
                ViewBag.Propiedades = new SelectList(_context.Propiedades, "PropiedadId", "Ubicacion", propiedadSeleccionada);
                return View(publicacion);
            }

            int vendedorId = GetVendedorIdAutenticado();
            if (vendedorId == 0) return Unauthorized();

            publicacion.VendedorId = vendedorId;
            publicacion.UsuarioId = _context.Vendedores
                                        .Where(v => v.VendedorId == vendedorId)
                                        .Select(v => v.UsuarioId)
                                        .FirstOrDefault();

            // Usar fecha del formulario si viene cargada, si no, DateTime.Now
            if (publicacion.FechaPublicacion == default)
                publicacion.FechaPublicacion = DateTime.Now;

            // Asignar Auto o Propiedad
            if (autoSeleccionado.HasValue)
            {
                publicacion.AutoId = autoSeleccionado.Value;
                publicacion.PropiedadId = null;
            }
            else if (propiedadSeleccionada.HasValue)
            {
                publicacion.PropiedadId = propiedadSeleccionada.Value;
                publicacion.AutoId = null;
            }

            // Validar imagen
            if (ImagenPrincipal != null && ImagenPrincipal.Length > 0)
            {
                var extension = Path.GetExtension(ImagenPrincipal.FileName).ToLower();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("ImagenPrincipal", "Formato de imagen no permitido. Solo JPG o PNG.");
                    ViewBag.Categorias = new SelectList(_context.Categorias, "CategoriaId", "Nombre", publicacion.CategoriaId);
                    ViewBag.Autos = new SelectList(_context.Autos, "AutoId", "Modelo", autoSeleccionado);
                    ViewBag.Propiedades = new SelectList(_context.Propiedades, "PropiedadId", "Ubicacion", propiedadSeleccionada);
                    return View(publicacion);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImagenPrincipal.CopyToAsync(stream);
                }

                publicacion.ImagenRuta = "/uploads/" + fileName;
            }

            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();

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
