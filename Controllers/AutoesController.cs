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
    public class AutoesController : Controller
    {
        private readonly ZonautoContext _context;
        private readonly IWebHostEnvironment _environment;

        public AutoesController(ZonautoContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ============================= INDEX =============================
        public async Task<IActionResult> Index()
        {
            var publicaciones = await _context.Publicaciones
                .Include(p => p.Auto)
                .Include(p => p.Categoria)
                .Include(p => p.Usuario)
                    .ThenInclude(u => u.Persona)
                .Where(p => p.Categoria.Tipo == "Auto")
                .ToListAsync();

            return View(publicaciones);
        }

        // ============================= DETAILS =============================
        public async Task<IActionResult> Details(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
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
                .Where(c => c.Tipo == "Auto")
                .ToList();

            var viewModel = new PublicacionCreateViewModel
            {
                Publicacione = new Publicacione
                {
                    Auto = new Auto()
                }
            };

            return View(viewModel.Publicacione);
        }

        // ============================= CREATE (POST) =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Marca,Modelo,Anio,Kilometro,Descripcion,CategoriaId")] Auto auto,
            decimal Precio,
            IFormFile? Imagen)
        {
            int? userId = HttpContext.Session.GetInt32("UsuarioId");

            Console.WriteLine($"🟡 [DEBUG] UsuarioId en sesión: {userId}");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para publicar.";
                return RedirectToAction("Login", "UserLogins");
            }

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(auto.Marca) ||
                string.IsNullOrWhiteSpace(auto.Modelo) ||
                Precio <= 0 ||
                auto.CategoriaId <= 0)
            {
                ModelState.AddModelError("", "Debe ingresar correctamente Marca, Modelo, Categoría y Precio.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = _context.Categorias
                    .Where(c => c.Tipo == "Auto")
                    .ToList();

                var viewModel = new PublicacionCreateViewModel
                {
                    Publicacione = new Publicacione { Auto = auto }
                };

                Console.WriteLine("❌ [DEBUG] ModelState inválido. No se guardó el auto.");
                return View(viewModel);
            }

            try
            {
                // Guardar imagen y asignar la ruta
                string? rutaImagen = await GuardarImagenAsync(Imagen);
                auto.ImagenRuta = rutaImagen; // ✅ AGREGADO: guarda la ruta en la base de datos

                // Guardar Auto con el usuario actual
                auto.UsuarioId = userId;
                _context.Autos.Add(auto);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ [DEBUG] Auto guardado correctamente con ID {auto.AutoId}");

                // Buscar o crear vendedor
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
                    Console.WriteLine($"✅ [DEBUG] Vendedor creado con ID {vendedor.VendedorId}");
                }

                // Crear publicación
                var publicacion = new Publicacione
                {
                    AutoId = auto.AutoId,
                    VendedorId = vendedor.VendedorId,
                    UsuarioId = userId,
                    CategoriaId = auto.CategoriaId,
                    Titulo = $"{auto.Marca} {auto.Modelo}",
                    Descripcion = auto.Descripcion,
                    Precio = Precio,
                    FechaPublicacion = DateTime.Now,
                    ImagenRuta = rutaImagen
                };

                _context.Publicaciones.Add(publicacion);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ [DEBUG] Publicación creada con ID {publicacion.PublicacionId}");

                TempData["SuccessMessage"] = "Publicación creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR] No se pudo guardar el auto: {ex.Message}");
                TempData["ErrorMessage"] = "Ocurrió un error al guardar la publicación. Verifique los datos.";
                return RedirectToAction(nameof(Create));
            }
        }

        // ============================= EDIT (GET) =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            ViewBag.Categorias = _context.Categorias
                .Where(c => c.Tipo == "Auto")
                .ToList();

            var viewModel = new PublicacionCreateViewModel
            {
                Publicacione = publicacion,
                Autos = _context.Autos.ToList()
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
                .Include(p => p.Auto)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacionDB == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(viewModel.Publicacione.Auto.Marca) ||
                string.IsNullOrWhiteSpace(viewModel.Publicacione.Auto.Modelo) ||
                viewModel.Publicacione.Precio <= 0)
            {
                ModelState.AddModelError("", "Debe ingresar correctamente Marca, Modelo y Precio.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = _context.Categorias
                    .Where(c => c.Tipo == "Auto")
                    .ToList();
                return View(viewModel);
            }

            // Imagen nueva si se subió
            string? rutaImagen = await GuardarImagenAsync(Imagen);
            if (!string.IsNullOrEmpty(rutaImagen))
            {
                publicacionDB.ImagenRuta = rutaImagen;
                publicacionDB.Auto.ImagenRuta = rutaImagen; // ✅ También se actualiza en el auto
            }

            // Actualizar Auto
            publicacionDB.Auto.Marca = viewModel.Publicacione.Auto.Marca;
            publicacionDB.Auto.Modelo = viewModel.Publicacione.Auto.Modelo;
            publicacionDB.Auto.Anio = viewModel.Publicacione.Auto.Anio;
            publicacionDB.Auto.Kilometro = viewModel.Publicacione.Auto.Kilometro;
            publicacionDB.Auto.Descripcion = viewModel.Publicacione.Auto.Descripcion;

            // Actualizar Publicación
            publicacionDB.Titulo = $"{viewModel.Publicacione.Auto.Marca} {viewModel.Publicacione.Auto.Modelo}";
            publicacionDB.Descripcion = viewModel.Publicacione.Descripcion;
            publicacionDB.Precio = viewModel.Publicacione.Precio;
            publicacionDB.FechaPublicacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Publicación actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================= DELETE (GET) =============================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
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
            var publicacion = await _context.Publicaciones
                .Include(p => p.Auto)
                .FirstOrDefaultAsync(p => p.PublicacionId == id);

            if (publicacion != null)
            {
                if (publicacion.Auto != null)
                    _context.Autos.Remove(publicacion.Auto);

                _context.Publicaciones.Remove(publicacion);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Publicación eliminada correctamente.";
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

            string carpeta = Path.Combine(_environment.WebRootPath, "images/autos");
            Directory.CreateDirectory(carpeta);

            string nombreArchivo = Guid.NewGuid() + extension;
            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                await Imagen.CopyToAsync(stream);

            return "/images/autos/" + nombreArchivo;
        }
    }
}
