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
    public class PublicacionesController : Controller
    {
        private readonly ZonautoContext _context;
        private readonly IWebHostEnvironment _environment;

        public PublicacionesController(ZonautoContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ============================= INDEX =============================
        public async Task<IActionResult> Index()
        {
            var publicaciones = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .Include(p => p.Categoria)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            return View(publicaciones);
        }

        // ============================= DETAILS =============================
        public async Task<IActionResult> Details(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .Include(p => p.Categoria)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            return View(publicacion);
        }

        // ============================= CREAR PUBLICACIÓN AUTO =============================
        [HttpGet]
        public IActionResult CrearPublicacionAuto()
        {
            // 🔒 Verificación de usuario
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para crear una publicación.";
                return RedirectToAction("Login", "UserLogins");
            }

            var viewModel = new PublicacionCreateViewModel
            {
                Publicacione = new Publicacione
                {
                    Auto = new Auto()
                },
                Autos = _context.Autos.ToList(),
                Propiedades = _context.Propiedades.ToList(),
                Categorias = _context.Categorias.Where(c => c.Tipo == "Auto").ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPublicacionAuto(PublicacionCreateViewModel model, IFormFile? Imagen)
        {
            // 🔒 Verificación de usuario
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "No tienes permiso para crear publicaciones.";
                return RedirectToAction("Login", "UserLogins");
            }

            int? userId = HttpContext.Session.GetInt32("UsuarioId");

            if (model.Publicacione?.Auto == null ||
                string.IsNullOrWhiteSpace(model.Publicacione.Auto.Marca) ||
                string.IsNullOrWhiteSpace(model.Publicacione.Auto.Modelo) ||
                model.Publicacione.Precio <= 0)
            {
                ModelState.AddModelError("", "Debe ingresar Marca, Modelo y Precio válidos.");
            }

            if (!ModelState.IsValid)
            {
                model.Categorias = _context.Categorias.Where(c => c.Tipo == "Auto").ToList();
                return View(model);
            }

            string? rutaImagen = await GuardarImagenAsync(Imagen, "autos");

            // Guardar el auto
            var auto = model.Publicacione.Auto;
            if (userId.HasValue)
                auto.UsuarioId = userId.Value;

            _context.Autos.Add(auto);
            await _context.SaveChangesAsync();

            // Crear la publicación
            var publicacion = new Publicacione
            {
                AutoId = auto.AutoId,
                UsuarioId = userId,
                CategoriaId = auto.CategoriaId,
                Titulo = model.Publicacione.Titulo ?? $"{auto.Marca} {auto.Modelo}",
                Descripcion = model.Publicacione.Descripcion ?? auto.Descripcion,
                Precio = model.Publicacione.Precio,
                FechaPublicacion = DateTime.Now,
                ImagenRuta = rutaImagen
            };

            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Publicación de Auto creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================= CREAR PUBLICACIÓN PROPIEDAD =============================
        [HttpGet]
        public IActionResult CrearPublicacionPropiedad()
        {
            // 🔒 Verificación de usuario
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para crear una publicación.";
                return RedirectToAction("Login", "UserLogins");
            }

            var viewModel = new PublicacionCreateViewModel
            {
                Publicacione = new Publicacione
                {
                    Propiedad = new Propiedade()
                },
                Autos = _context.Autos.ToList(),
                Propiedades = _context.Propiedades.ToList(),
                Categorias = _context.Categorias.Where(c => c.Tipo == "Propiedad").ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPublicacionPropiedad(PublicacionCreateViewModel model, IFormFile? Imagen)
        {
            // 🔒 Verificación de usuario
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "No tienes permiso para crear publicaciones.";
                return RedirectToAction("Login", "UserLogins");
            }

            int? userId = HttpContext.Session.GetInt32("UsuarioId");

            if (model.Publicacione?.Propiedad == null ||
                string.IsNullOrWhiteSpace(model.Publicacione.Propiedad.Ubicacion) ||
                model.Publicacione.Precio <= 0)
            {
                ModelState.AddModelError("", "Debe ingresar Ubicación y Precio válidos.");
            }

            if (!ModelState.IsValid)
            {
                model.Categorias = _context.Categorias.Where(c => c.Tipo == "Propiedad").ToList();
                return View(model);
            }

            string? rutaImagen = await GuardarImagenAsync(Imagen, "propiedades");

            // Guardar la propiedad
            var propiedad = model.Publicacione.Propiedad;
            if (userId.HasValue)
                propiedad.UsuarioId = userId.Value;

            _context.Propiedades.Add(propiedad);
            await _context.SaveChangesAsync();

            // Crear publicación
            var publicacion = new Publicacione
            {
                PropiedadId = propiedad.PropiedadId,
                UsuarioId = userId,
                CategoriaId = propiedad.CategoriaId,
                Titulo = model.Publicacione.Titulo ?? $"{propiedad.Tipo} en {propiedad.Ubicacion}",
                Descripcion = model.Publicacione.Descripcion ?? propiedad.Descripcion,
                Precio = model.Publicacione.Precio,
                FechaPublicacion = DateTime.Now,
                ImagenRuta = rutaImagen
            };

            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Publicación de Propiedad creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================= EDITAR PUBLICACIÓN =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 🔒 Solo logueados
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "No tienes permiso para editar publicaciones.";
                return RedirectToAction("Login", "UserLogins");
            }

            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            return View(publicacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publicacione model, IFormFile? Imagen)
        {
            // 🔒 Solo logueados
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "No tienes permiso para editar publicaciones.";
                return RedirectToAction("Login", "UserLogins");
            }

            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            publicacion.Descripcion = model.Descripcion;
            publicacion.Precio = model.Precio;
            publicacion.Titulo = model.Titulo ?? publicacion.Titulo;
            publicacion.FechaPublicacion = DateTime.Now;

            string? rutaImagen = await GuardarImagenAsync(Imagen,
                publicacion.Auto != null ? "autos" : "propiedades");

            if (!string.IsNullOrEmpty(rutaImagen))
                publicacion.ImagenRuta = rutaImagen;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Publicación actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================= ELIMINAR PUBLICACIÓN =============================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // 🔒 Solo logueados
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "No tienes permiso para eliminar publicaciones.";
                return RedirectToAction("Login", "UserLogins");
            }

            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            return View(publicacion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 🔒 Solo logueados
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario == "Invitado")
            {
                TempData["ErrorMessage"] = "No tienes permiso para eliminar publicaciones.";
                return RedirectToAction("Login", "UserLogins");
            }

            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion != null)
            {
                if (publicacion.Auto != null)
                    _context.Autos.Remove(publicacion.Auto);

                if (publicacion.Propiedad != null)
                    _context.Propiedades.Remove(publicacion.Propiedad);

                _context.Publicaciones.Remove(publicacion);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Publicación eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================= MÉTODO PRIVADO PARA GUARDAR IMAGEN =============================
        private async Task<string?> GuardarImagenAsync(IFormFile? Imagen, string tipo)
        {
            if (Imagen == null || Imagen.Length == 0) return null;

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(Imagen.FileName).ToLower();

            if (!extensionesPermitidas.Contains(extension))
                return null;

            string carpeta = tipo.ToLower() == "autos"
                ? Path.Combine(_environment.WebRootPath, "images", "autos")
                : Path.Combine(_environment.WebRootPath, "images", "propiedades");

            Directory.CreateDirectory(carpeta);

            string nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await Imagen.CopyToAsync(stream);
            }

            return tipo.ToLower() == "autos"
                ? $"/images/autos/{nombreArchivo}"
                : $"/images/propiedades/{nombreArchivo}";
        }
    }
}
