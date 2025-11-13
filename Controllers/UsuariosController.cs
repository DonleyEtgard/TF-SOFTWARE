using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ZONAUTO.config;

namespace ZONAUTO.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ZonautoContext _context;
        private readonly EmailSettings _emailSettings;

        public UsuariosController(ZonautoContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value;
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .ThenInclude(p => p.Direcciones)
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .ThenInclude(p => p.Direcciones)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null) return NotFound();

            CargarTipoUsuarios(usuario.TipoUsuarioId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nombre, string apellido, string email, DateTime? fechaNacimiento,
            string direccion, string ciudad, string provincia, string codigoPostal, string pais, int tipoUsuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .ThenInclude(p => p.Direcciones)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null) return NotFound();

            if (!CamposValidos(nombre, apellido, email, fechaNacimiento, direccion, ciudad, provincia, codigoPostal, pais, "dummy"))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                CargarTipoUsuarios(tipoUsuarioId);
                return View(usuario);
            }

            try
            {
                usuario.Persona.Nombre = nombre;
                usuario.Persona.Apellido = apellido;
                usuario.Persona.Email = email;
                usuario.Persona.FechaNacimiento = DateOnly.FromDateTime(fechaNacimiento.Value);

                // Tomamos la primera dirección de la colección
                var direccionObj = usuario.Persona.Direcciones.FirstOrDefault();
                if (direccionObj != null)
                {
                    direccionObj.Calle = direccion;
                    direccionObj.Ciudad = ciudad;
                    direccionObj.Provincia = provincia;
                    direccionObj.CodigoPostal = codigoPostal;
                    direccionObj.Pais = pais;
                }

                usuario.TipoUsuarioId = tipoUsuarioId;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario actualizado correctamente.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar usuario: {ex.Message}");
                CargarTipoUsuarios(tipoUsuarioId);
                return View(usuario);
            }
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .ThenInclude(p => p.Direcciones)
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .ThenInclude(p => p.Direcciones)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null) return NotFound();

            try
            {
                var direccionObj = usuario.Persona.Direcciones.FirstOrDefault();
                if (direccionObj != null)
                    _context.Direcciones.Remove(direccionObj);

                _context.Personas.Remove(usuario.Persona);
                _context.Usuarios.Remove(usuario);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario eliminado correctamente.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar usuario: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Usuarios/RegistrarUsuario
        public IActionResult RegistrarUsuario()
        {
            CargarTipoUsuarios();
            return View();
        }

        // POST: Usuarios/RegistrarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarUsuario(
            string nombre, string apellido, string email, DateTime? fechaNacimiento,
            string direccion, string ciudad, string provincia, string codigoPostal,
            string pais, string contrasena, string confirmarContrasena, int tipoUsuarioId)
        {
            if (contrasena != confirmarContrasena)
                ModelState.AddModelError("confirmarContrasena", "Las contraseñas no coinciden.");

            if (!CamposValidos(nombre, apellido, email, fechaNacimiento, direccion,
                               ciudad, provincia, codigoPostal, pais, contrasena))
                ModelState.AddModelError("", "Todos los campos son obligatorios.");

            if (!await _context.TiposUsuarios.AnyAsync(t => t.TipoUsuarioId == tipoUsuarioId))
                ModelState.AddModelError("tipoUsuarioId", "Tipo de usuario inválido.");

            if (await _context.Personas.AnyAsync(p => p.Email == email))
                ModelState.AddModelError("email", "El email ya está registrado.");

            if (!ModelState.IsValid)
            {
                CargarTipoUsuarios(tipoUsuarioId);
                return View();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var persona = new Persona
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = email,
                    FechaNacimiento = DateOnly.FromDateTime(fechaNacimiento.Value)
                };
                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                var direccionObj = new Direccione
                {
                    Calle = direccion,
                    Ciudad = ciudad,
                    Provincia = provincia,
                    CodigoPostal = codigoPostal,
                    Pais = pais,
                    PersonaId = persona.PersonaId
                };
                _context.Direcciones.Add(direccionObj);
                await _context.SaveChangesAsync();

                persona.DireccionId = direccionObj.DireccionId;
                await _context.SaveChangesAsync();

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(contrasena);
                var tokenVerificacion = Guid.NewGuid().ToString();

                var usuario = new Usuario
                {
                    PersonaId = persona.PersonaId,
                    ContrasenaHash = hashedPassword,
                    FechaRegistro = DateTime.Now,
                    Habilitado = true,
                    Verificado = false,
                    TokenVerificacion = tokenVerificacion,
                    TipoUsuarioId = tipoUsuarioId
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                await CrearEntidadPorTipo(tipoUsuarioId, usuario.UsuarioId);
                await transaction.CommitAsync();

                await EnviarEmailConfirmacionAsync(email, nombre, tokenVerificacion);

                HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
                TempData["UsuarioNombre"] = nombre;

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error al registrar usuario: {ex.Message}");
                CargarTipoUsuarios(tipoUsuarioId);
                return View();
            }
        }
        private async Task EnviarEmailBienvenidaAsync(string email, string nombre)
        {
            try
            {
                var fromAddress = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                var toAddress = new MailAddress(email, string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre);

                string body = $@"
        <html>
        <body>
            ¡Hola {nombre}!<br/><br/>
            🎉 Tu cuenta en <b>ZonaAuto</b> ha sido confirmada exitosamente.<br/>
            Ahora puedes acceder a todas las funcionalidades de la plataforma.<br/><br/>
            ¡Bienvenido!
        </body>
        </html>";

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "¡Bienvenido a ZonaAuto!",
                    Body = body,
                    IsBodyHtml = true
                };

                using var smtp = new SmtpClient
                {
                    Host = _emailSettings.SmtpServer,
                    Port = _emailSettings.SmtpPort,
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error enviando email de bienvenida: " + ex.Message);
            }
        }


        // GET: Usuarios/CambiarContrasena
        public IActionResult CambiarContrasena() => View();

        // POST: Usuarios/CambiarContrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(
            string email, string nombre, DateTime? fechaNacimiento,
            string nuevaContrasena, string confirmarContrasena)
        {
            // Buscar el usuario por email
            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Persona.Email == email);

            if (usuario == null)
            {
                ModelState.AddModelError("email", "No se encontró un usuario con ese email.");
                return View();
            }

            // Validar nombre
            if (!string.Equals(usuario.Persona.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("nombre", "El nombre no coincide con el registrado.");
                return View();
            }

            // Validar fecha de nacimiento
            if (!fechaNacimiento.HasValue || usuario.Persona.FechaNacimiento != DateOnly.FromDateTime(fechaNacimiento.Value))
            {
                ModelState.AddModelError("fechaNacimiento", "La fecha de nacimiento no coincide.");
                return View();
            }

            // Validar nueva contraseña
            if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 6)
            {
                ModelState.AddModelError("nuevaContrasena", "La nueva contraseña debe tener al menos 6 caracteres.");
                return View();
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                ModelState.AddModelError("confirmarContrasena", "Las contraseñas no coinciden.");
                return View();
            }

            // Guardar nueva contraseña
            usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tu contraseña fue restablecida correctamente.";
            return RedirectToAction("Login", "Auth");
        }


        // GET: Usuarios/ConfirmarCuenta?token=xxxx
        public async Task<IActionResult> ConfirmarCuenta(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.TokenVerificacion == token);

            if (usuario == null)
                return NotFound();

            usuario.Verificado = true;
            usuario.Habilitado = true;
            usuario.TokenVerificacion = null;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
            TempData["Success"] = $"Cuenta confirmada correctamente. Bienvenido, {usuario.Persona.Nombre}!";

            return RedirectToAction("Index", "Home");
        }

        // Helpers
        private bool CamposValidos(string nombre, string apellido, string email, DateTime? fechaNacimiento,
                                   string direccion, string ciudad, string provincia, string codigoPostal,
                                   string pais, string contrasena)
        {
            return !(string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) ||
                     string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasena) ||
                     string.IsNullOrWhiteSpace(direccion) || string.IsNullOrWhiteSpace(ciudad) ||
                     string.IsNullOrWhiteSpace(provincia) || string.IsNullOrWhiteSpace(pais) ||
                     string.IsNullOrWhiteSpace(codigoPostal) || !fechaNacimiento.HasValue);
        }

        private void CargarTipoUsuarios(int tipoSeleccionado = 0)
        {
            ViewData["TipoUsuarioId"] = new SelectList(_context.TiposUsuarios, "TipoUsuarioId", "Nombre", tipoSeleccionado);
        }

        private async Task CrearEntidadPorTipo(int tipoUsuarioId, int usuarioId)
        {
            if (tipoUsuarioId == 1)
                _context.Administradores.Add(new Administradore { UsuarioId = usuarioId });
            else if (tipoUsuarioId == 2)
                _context.Vendedores.Add(new Vendedore { UsuarioId = usuarioId, ServicioPremium = false });
            else if (tipoUsuarioId == 3)
                _context.Compradores.Add(new Compradore { UsuarioId = usuarioId });

            await _context.SaveChangesAsync();
        }

        private async Task EnviarEmailConfirmacionAsync(string email, string nombre, string token)
        {
            try
            {
                var urlConfirmacion = Url.Action("ConfirmarCuenta", "Usuarios", new { token }, Request.Scheme);

                var fromAddress = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                var toAddress = new MailAddress(email, string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre);

                string body = $@"
                <html>
                <body>
                    Hola {nombre},<br/><br/>
                    Gracias por registrarte. Por favor confirma tu cuenta haciendo clic en el siguiente enlace:<br/>
                    <a href='{urlConfirmacion}'>Confirmar Cuenta</a><br/><br/>
                    Si no solicitaste este registro, ignora este correo.
                </body>
                </html>";

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Confirma tu cuenta en ZonaAuto",
                    Body = body,
                    IsBodyHtml = true
                };

                using var smtp = new SmtpClient
                {
                    Host = _emailSettings.SmtpServer,
                    Port = _emailSettings.SmtpPort,
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error enviando email: " + ex.Message);
            }
        }
    }
}
