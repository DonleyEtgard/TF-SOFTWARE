using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

namespace ZONAUTO.Controllers
{
    public class TiposUsuariosController : Controller
    {
        private readonly ZonautoContext _context;

        public TiposUsuariosController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: TiposUsuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.TiposUsuarios.ToListAsync());
        }

        // GET: TiposUsuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposUsuario = await _context.TiposUsuarios
                .FirstOrDefaultAsync(m => m.TipoUsuarioId == id);
            if (tiposUsuario == null)
            {
                return NotFound();
            }

            return View(tiposUsuario);
        }

        // GET: TiposUsuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TiposUsuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoUsuarioId,Nombre")] TiposUsuario tiposUsuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tiposUsuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tiposUsuario);
        }

        // GET: TiposUsuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposUsuario = await _context.TiposUsuarios.FindAsync(id);
            if (tiposUsuario == null)
            {
                return NotFound();
            }
            return View(tiposUsuario);
        }

        // POST: TiposUsuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TipoUsuarioId,Nombre")] TiposUsuario tiposUsuario)
        {
            if (id != tiposUsuario.TipoUsuarioId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tiposUsuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TiposUsuarioExists(tiposUsuario.TipoUsuarioId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tiposUsuario);
        }

        // GET: TiposUsuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposUsuario = await _context.TiposUsuarios
                .FirstOrDefaultAsync(m => m.TipoUsuarioId == id);
            if (tiposUsuario == null)
            {
                return NotFound();
            }

            return View(tiposUsuario);
        }

        // POST: TiposUsuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tiposUsuario = await _context.TiposUsuarios.FindAsync(id);
            if (tiposUsuario != null)
            {
                _context.TiposUsuarios.Remove(tiposUsuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TiposUsuarioExists(int id)
        {
            return _context.TiposUsuarios.Any(e => e.TipoUsuarioId == id);
        }
    }
}
