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
    public class MensajesController : Controller
    {
        private readonly ZonautoContext _context;

        public MensajesController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: Mensajes
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.Mensajes.Include(m => m.Chat).Include(m => m.Usuario);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: Mensajes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensaje = await _context.Mensajes
                .Include(m => m.Chat)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.MensajeId == id);
            if (mensaje == null)
            {
                return NotFound();
            }

            return View(mensaje);
        }

        // GET: Mensajes/Create
        public IActionResult Create()
        {
            ViewData["ChatId"] = new SelectList(_context.Chats, "ChatId", "ChatId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: Mensajes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MensajeId,ChatId,UsuarioId,Contenido,Fecha")] Mensaje mensaje)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mensaje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChatId"] = new SelectList(_context.Chats, "ChatId", "ChatId", mensaje.ChatId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", mensaje.UsuarioId);
            return View(mensaje);
        }

        // GET: Mensajes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensaje = await _context.Mensajes.FindAsync(id);
            if (mensaje == null)
            {
                return NotFound();
            }
            ViewData["ChatId"] = new SelectList(_context.Chats, "ChatId", "ChatId", mensaje.ChatId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", mensaje.UsuarioId);
            return View(mensaje);
        }

        // POST: Mensajes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MensajeId,ChatId,UsuarioId,Contenido,Fecha")] Mensaje mensaje)
        {
            if (id != mensaje.MensajeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mensaje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MensajeExists(mensaje.MensajeId))
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
            ViewData["ChatId"] = new SelectList(_context.Chats, "ChatId", "ChatId", mensaje.ChatId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", mensaje.UsuarioId);
            return View(mensaje);
        }

        // GET: Mensajes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensaje = await _context.Mensajes
                .Include(m => m.Chat)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.MensajeId == id);
            if (mensaje == null)
            {
                return NotFound();
            }

            return View(mensaje);
        }

        // POST: Mensajes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mensaje = await _context.Mensajes.FindAsync(id);
            if (mensaje != null)
            {
                _context.Mensajes.Remove(mensaje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MensajeExists(int id)
        {
            return _context.Mensajes.Any(e => e.MensajeId == id);
        }
    }
}
