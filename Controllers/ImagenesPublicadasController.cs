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
    public class ImagenesPublicadasController : Controller
    {
        private readonly ZonautoContext _context;

        public ImagenesPublicadasController(ZonautoContext context)
        {
            _context = context;
        }

        // GET: ImagenesPublicadas
        public async Task<IActionResult> Index()
        {
            var zonautoContext = _context.ImagenesPublicada.Include(i => i.Publicacion);
            return View(await zonautoContext.ToListAsync());
        }

        // GET: ImagenesPublicadas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenesPublicada = await _context.ImagenesPublicada
                .Include(i => i.Publicacion)
                .FirstOrDefaultAsync(m => m.ImagenId == id);
            if (imagenesPublicada == null)
            {
                return NotFound();
            }

            return View(imagenesPublicada);
        }

        // GET: ImagenesPublicadas/Create
        public IActionResult Create()
        {
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId");
            return View();
        }

        // POST: ImagenesPublicadas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImagenId,PublicacionId,UrlImagen,EsPrincipal,FechaSubida")] ImagenesPublicada imagenesPublicada)
        {
            if (ModelState.IsValid)
            {
                _context.Add(imagenesPublicada);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", imagenesPublicada.PublicacionId);
            return View(imagenesPublicada);
        }

        // GET: ImagenesPublicadas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenesPublicada = await _context.ImagenesPublicada.FindAsync(id);
            if (imagenesPublicada == null)
            {
                return NotFound();
            }
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", imagenesPublicada.PublicacionId);
            return View(imagenesPublicada);
        }

        // POST: ImagenesPublicadas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ImagenId,PublicacionId,UrlImagen,EsPrincipal,FechaSubida")] ImagenesPublicada imagenesPublicada)
        {
            if (id != imagenesPublicada.ImagenId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imagenesPublicada);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImagenesPublicadaExists(imagenesPublicada.ImagenId))
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
            ViewData["PublicacionId"] = new SelectList(_context.Publicaciones, "PublicacionId", "PublicacionId", imagenesPublicada.PublicacionId);
            return View(imagenesPublicada);
        }

        // GET: ImagenesPublicadas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenesPublicada = await _context.ImagenesPublicada
                .Include(i => i.Publicacion)
                .FirstOrDefaultAsync(m => m.ImagenId == id);
            if (imagenesPublicada == null)
            {
                return NotFound();
            }

            return View(imagenesPublicada);
        }

        // POST: ImagenesPublicadas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imagenesPublicada = await _context.ImagenesPublicada.FindAsync(id);
            if (imagenesPublicada != null)
            {
                _context.ImagenesPublicada.Remove(imagenesPublicada);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImagenesPublicadaExists(int id)
        {
            return _context.ImagenesPublicada.Any(e => e.ImagenId == id);
        }
    }
}
