using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministradoresController : Controller
    {
        private readonly ZonautoContext _context;
        private const int PageSize = 10; // Para paginación

        public AdministradoresController(ZonautoContext context)
        {
            _context = context;
        }

        #region Dashboard y Gestión de Usuarios

        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = new
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                TotalAdministradores = await _context.Administradores.CountAsync(),
                TotalPublicaciones = await _context.Publicaciones.CountAsync(),
                TotalOfertas = await _context.Ofertas.CountAsync(),
                UltimosUsuarios = await _context.Usuarios
                    .Include(u => u.Persona)
                    .OrderByDescending(u => u.UsuarioId)
                    .Take(5)
                    .ToListAsync(),
                UltimasOfertas = await _context.Ofertas
                    .OrderByDescending(o => o.OfertaId)
                    .Take(5)
                    .ToListAsync(),
                PublicacionesPorMes = await GetPublicacionesPorMes()
            };

            return View(dashboardData);
        }

        public async Task<IActionResult> GestionUsuarios(int page = 1, string filtroRol = null)
        {
            var query = _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.TipoUsuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtroRol))
            {
                query = query.Where(u => u.TipoUsuario.Nombre.Contains(filtroRol));
            }

            var usuarios = await query
                .OrderByDescending(u => u.FechaRegistro)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);
            ViewBag.FiltroRol = filtroRol;

            return View(usuarios);
        }
        [HttpGet]
        public IActionResult BuscarPublicaciones(string query)
        {
            var publicaciones = _context.Publicaciones
                .Include(p => p.Usuario)
                .Where(p => string.IsNullOrEmpty(query) ||
                            p.Titulo.Contains(query) ||
                            p.Descripcion.Contains(query))
                .ToList();

            return View("Publicaciones", publicaciones);
        }
     
    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Habilitado = !usuario.Habilitado;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Usuario {(usuario.Habilitado ? "habilitado" : "deshabilitado")} correctamente.";
            return RedirectToAction(nameof(GestionUsuarios));
        }

        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.TipoUsuario)
                .Include(u => u.Direccion)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            return usuario == null ? NotFound() : View(usuario);
        }

        #endregion

        #region CRUD Administradores

        public async Task<IActionResult> Index()
        {
            return View(await _context.Administradores
                .Include(a => a.Usuario)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdministradorId,UsuarioId")] Administradore administradore)
        {
            if (ModelState.IsValid)
            {
                _context.Add(administradore);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "Email", administradore.UsuarioId);
            return View(administradore);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var administradore = await _context.Administradores.FindAsync(id);
            if (administradore == null) return NotFound();

            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "Email", administradore.UsuarioId);
            return View(administradore);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdministradorId,UsuarioId")] Administradore administradore)
        {
            if (id != administradore.AdministradorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administradore);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministradoreExists(administradore.AdministradorId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "Email", administradore.UsuarioId);
            return View(administradore);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var administradore = await _context.Administradores
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.AdministradorId == id);

            return administradore == null ? NotFound() : View(administradore);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var administradore = await _context.Administradores.FindAsync(id);
            if (administradore != null)
            {
                _context.Administradores.Remove(administradore);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Métodos Privados

        private bool AdministradoreExists(int id)
        {
            return _context.Administradores.Any(e => e.AdministradorId == id);
        }

        private async Task<object> GetPublicacionesPorMes()
        {
            return await _context.Publicaciones
                .GroupBy(p => new { p.FechaPublicacion.Year, p.FechaPublicacion.Month })
                .Select(g => new
                {
                    Mes = g.Key.Month,
                    Cantidad = g.Count()
                })
                .OrderBy(g => g.Mes)
                .ToListAsync();
        }

        #endregion
    }
}