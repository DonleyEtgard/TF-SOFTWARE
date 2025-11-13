using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class PropiedadesController : Controller
    {
        private readonly ZonautoContext _context;
        private readonly IWebHostEnvironment _environment;

        public PropiedadesController(ZonautoContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ============================= INDEX =============================
        public async Task<IActionResult> Index()
        {
            var publicaciones = await _context.Publicaciones
                .Include(p => p.Propiedad)
                .Include(p => p.Categoria)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .Where(p => p.Categoria.Tipo == "Propiedad")
                .ToListAsync();

            return View(publicaciones); // La vista debe usar: @model IEnumerable<ZONAUTO.Models.Publicacione>
        }

        // ============================= DETAILS =============================
        public async Task<IActionResult> Details(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Propiedad)
                .Include(p => p.Categoria)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            return View(publicacion);
        }

        // ============================= CREATE (GET) =============================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categorias = _context.Categorias
                .Where(c => c.Tipo == "Propiedad")
                .ToList();

            var viewModel = new PublicacionCreateViewModel
            {
                Publicacione = new Publicacione
                {
                    Propiedad = new Propiedade()
                }
            };

            return View(viewModel);
        }

        // ============================= CREATE (POST) =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublicacionCreateViewModel viewModel, IFormFile? Imagen)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para publicar.";
                return RedirectToAction("Login", "UserLogins");
            }

            var propiedad = viewModel.Publicacione.Propiedad;
            var publicacion = viewModel.Publicacione;
            decimal precio = publicacion.Precio;

            // 🔍 Validaciones básicas
            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(propiedad.Ubicacion) ||
                propiedad.MetroCuadrados <= 0 ||
                precio <= 0 ||
                propiedad.CategoriaId <= 0)
            {
                ModelState.AddModelError("", "Debe ingresar correctamente todos los datos.");
                ViewBag.Categorias = _context.Categorias.Where(c => c.Tipo == "Propiedad").ToList();
                return View(viewModel);
            }

            try
            {
                // 📸 Guardar imagen (si hay)
                string? rutaImagen = await GuardarImagenAsync(Imagen);
                propiedad.ImagenRuta = rutaImagen;
                propiedad.UsuarioId = userId;

                // 💾 Guardar la propiedad primero
                _context.Propiedades.Add(propiedad);
                await _context.SaveChangesAsync();

                // 🔎 Verificar si el usuario ya es vendedor
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UsuarioId == userId);
                if (vendedor == null)
                {
                    vendedor = new Vendedore
                    {
                        UsuarioId = userId.Value,
                        ServicioPremium = false
                    };
                    _context.Vendedores.Add(vendedor);
                    await _context.SaveChangesAsync();
                }

                // 🏠 Crear publicación asociada
                var nuevaPublicacion = new Publicacione
                {
                    PropiedadId = propiedad.PropiedadId,
                    VendedorId = vendedor.VendedorId,
                    UsuarioId = userId,
                    CategoriaId = propiedad.CategoriaId,
                    Titulo = string.IsNullOrWhiteSpace(publicacion.Titulo)
                                ? $"{propiedad.Tipo} en {propiedad.Ubicacion}"
                                : publicacion.Titulo,
                    Descripcion = publicacion.Descripcion ?? propiedad.Descripcion,
                    Precio = precio,
                    FechaPublicacion = DateTime.Now,
                    ImagenRuta = rutaImagen
                };

                _context.Publicaciones.Add(nuevaPublicacion);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Publicación creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR] No se pudo guardar la propiedad: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al guardar la publicación.";
                ViewBag.Categorias = _context.Categorias.Where(c => c.Tipo == "Propiedad").ToList();
                return View(viewModel);
            }
        }

        // ============================= EDIT (GET) =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Propiedad)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            ViewBag.Categorias = _context.Categorias
                .Where(c => c.Tipo == "Propiedad")
                .ToList();

            var viewModel = new PublicacionCreateViewModel
            {
                Publicacione = publicacion,
                Propiedades = _context.Propiedades.ToList()
            };

            return View(viewModel);
        }

        // ============================= EDIT (POST) =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PublicacionCreateViewModel viewModel, IFormFile? Imagen)
        {
            if (id != viewModel.Publicacione.PublicacionId)
                return NotFound();

            var publicacionDB = await _context.Publicaciones
                .Include(p => p.Propiedad)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacionDB == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = _context.Categorias.Where(c => c.Tipo == "Propiedad").ToList();
                return View(viewModel);
            }

            try
            {
                string? rutaImagen = await GuardarImagenAsync(Imagen);
                if (!string.IsNullOrEmpty(rutaImagen))
                {
                    publicacionDB.ImagenRuta = rutaImagen;
                    publicacionDB.Propiedad.ImagenRuta = rutaImagen;
                }

                publicacionDB.Propiedad.Ubicacion = viewModel.Publicacione.Propiedad.Ubicacion;
                publicacionDB.Propiedad.MetroCuadrados = viewModel.Publicacione.Propiedad.MetroCuadrados;
                publicacionDB.Propiedad.Habitaciones = viewModel.Publicacione.Propiedad.Habitaciones;
                publicacionDB.Propiedad.Tipo = viewModel.Publicacione.Propiedad.Tipo;
                publicacionDB.Propiedad.Descripcion = viewModel.Publicacione.Propiedad.Descripcion;

                publicacionDB.Titulo = $"{publicacionDB.Propiedad.Tipo} en {publicacionDB.Propiedad.Ubicacion}";
                publicacionDB.Descripcion = viewModel.Publicacione.Descripcion;
                publicacionDB.Precio = viewModel.Publicacione.Precio;
                publicacionDB.FechaPublicacion = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Publicación actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR] No se pudo editar la publicación: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar la publicación.";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // ============================= DELETE (GET) =============================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Propiedad)
                .Include(p => p.Categoria)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            return View(publicacion);
        }

        // ============================= DELETE (POST) =============================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var publicacion = await _context.Publicaciones
                    .Include(p => p.Propiedad)
                    .FirstOrDefaultAsync(p => p.PublicacionId == id);

                if (publicacion != null)
                {
                    if (publicacion.Propiedad != null)
                        _context.Propiedades.Remove(publicacion.Propiedad);

                    _context.Publicaciones.Remove(publicacion);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Publicación eliminada correctamente.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR] No se pudo eliminar la publicación: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la publicación.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================= MÉTODO PRIVADO =============================
        private async Task<string?> GuardarImagenAsync(IFormFile? Imagen)
        {
            if (Imagen == null || Imagen.Length == 0)
                return null;

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(Imagen.FileName).ToLower();

            if (!extensionesPermitidas.Contains(extension))
                return null;

            string carpeta = Path.Combine(_environment.WebRootPath, "images/propiedades");
            Directory.CreateDirectory(carpeta);

            string nombreArchivo = Guid.NewGuid() + extension;
            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                await Imagen.CopyToAsync(stream);

            return "/images/propiedades/" + nombreArchivo;
        }
    }
}

