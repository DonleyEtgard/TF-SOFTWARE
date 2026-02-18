using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ZonautoContext _context;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, ZonautoContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;
        }

        public IActionResult Index(bool autenticado = false)
        {
            if (autenticado && User.Identity.IsAuthenticated)
            {
                ViewBag.MensajeBienvenida = $"Bienvenido {User.Identity.Name}";
                ViewBag.TipoUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
            }

            // ?? Leer imágenes de autos
            string autosPath = Path.Combine(_env.WebRootPath, "images", "autos");
            var autos = Directory.Exists(autosPath)
                ? Directory.GetFiles(autosPath)
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .Select(f => "/images/autos/" + Path.GetFileName(f))
                    .ToList()
                : new List<string>();

            // ?? Leer imágenes de autos
            string propiedadesPath = Path.Combine(_env.WebRootPath, "images", "propiedades");
            var propiedades = Directory.Exists(propiedadesPath)
                ? Directory.GetFiles(propiedadesPath)
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .Select(f => "/images/propiedades/" + Path.GetFileName(f))
                    .ToList()
                : new List<string>();

            // Pasamos al ViewBag para usarlos en Index.cshtml
            ViewBag.Autos = autos;
            ViewBag.Propiedades = propiedades;

            return View();
        }

        // Acción para subir fotos
        [HttpPost]
        public async Task<IActionResult> SubirFoto(IFormFile archivo, int publicacionId)
        {
            if (archivo != null && archivo.Length > 0)
            {
                string carpetaDestino = Path.Combine(_env.WebRootPath, "images", "publicaciones");
                if (!Directory.Exists(carpetaDestino))
                    Directory.CreateDirectory(carpetaDestino);

                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
                string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                var publicacion = await _context.Publicaciones.FindAsync(publicacionId);
                if (publicacion != null)
                {
                    publicacion.ImagenRuta = $"/images/publicaciones/{nombreArchivo}";
                    _context.Update(publicacion);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        // Acción básica para buscador
        [HttpGet]
        public IActionResult BuscarPublicaciones(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var resultados = _context.Publicaciones
                .Where(p => p.Titulo.Contains(query) || p.Descripcion.Contains(query))
                .ToList();

            return View("ResultadosBusqueda", resultados);
        }

        // Acción avanzada de búsqueda
        [HttpGet]
        public IActionResult BuscarPublicacionesAvanzado(
            string? categoria = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            string? marca = null,
            string? modelo = null,
            string? ubicacion = null,
            string? nombreVendedor = null)
        {
            var publicaciones = _context.Publicaciones
                .Include(p => p.Usuario).ThenInclude(u => u.Persona)
                .Include(p => p.Auto)
                .Include(p => p.Propiedad)
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
                publicaciones = publicaciones.Where(p => p.Categoria.Nombre.Contains(categoria));

            if (precioMin.HasValue)
                publicaciones = publicaciones.Where(p => p.Precio >= precioMin.Value);
            if (precioMax.HasValue)
                publicaciones = publicaciones.Where(p => p.Precio <= precioMax.Value);

            if (!string.IsNullOrEmpty(marca))
                publicaciones = publicaciones.Where(p => p.Auto != null && p.Auto.Marca.Contains(marca));
            if (!string.IsNullOrEmpty(modelo))
                publicaciones = publicaciones.Where(p => p.Auto != null && p.Auto.Modelo.Contains(modelo));

            if (!string.IsNullOrEmpty(ubicacion))
                publicaciones = publicaciones.Where(p => p.Propiedad != null && p.Propiedad.Ubicacion.Contains(ubicacion));

            if (!string.IsNullOrEmpty(nombreVendedor))
                publicaciones = publicaciones.Where(p =>
                    p.Usuario != null &&
                    (p.Usuario.Persona.Nombre.Contains(nombreVendedor) ||
                     p.Usuario.Persona.Apellido.Contains(nombreVendedor)));

            var resultados = publicaciones.ToList();

            return View("ResultadosBusqueda", resultados);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}