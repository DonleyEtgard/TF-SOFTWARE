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
            _emailSettings = emailSettings.Value; // Carga datos desde appsettings.json
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
            // Validaciones
            if (contrasena != confirmarContrasena)
                ModelState.AddModelError("confirmarContrasena", "Las contraseñas no coinciden.");

            if (!CamposValidos(nombre, apellido, email, fechaNacimiento, direccion,
                               ciudad, provincia, codigoPostal, pais, contrasena))
                ModelState.AddModelError("", "Todos los campos son obligatorios.");

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
                // Crear Persona
                var persona = new Persona
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = email,
                    FechaNacimiento = DateOnly.FromDateTime(fechaNacimiento.Value)
                };
                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                // Crear Dirección
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

                // Crear Usuario
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

                // Crear entidad por tipo
                await CrearEntidadPorTipo(tipoUsuarioId, usuario.UsuarioId);

                await transaction.CommitAsync();

                // Enviar email de confirmación
                await EnviarEmailConfirmacionAsync(email, nombre, tokenVerificacion);

                TempData["UsuarioNombre"] = nombre; // Guardar nombre del usuario
                return RedirectToAction("Index", "Home"); // Redirigir a página principal
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error al registrar usuario: {ex.Message}");
                CargarTipoUsuarios(tipoUsuarioId);
                return View();
            }
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
            usuario.TokenVerificacion = null; // ya no se puede reutilizar
            await _context.SaveChangesAsync();

            // Guardar el nombre en sesión
            HttpContext.Session.SetString("NombreUsuario", usuario.Persona.Nombre);

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
            switch (tipoUsuarioId)
            {
                case 1: _context.Administradores.Add(new Administradore { UsuarioId = usuarioId }); break;
                case 2: _context.Vendedores.Add(new Vendedore { UsuarioId = usuarioId, ServicioPremium = false }); break;
                case 3: _context.Compradores.Add(new Compradore { UsuarioId = usuarioId }); break;
            }
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

                using var smtp = new SmtpClient
                {
                    Host = _emailSettings.SmtpServer,
                    Port = _emailSettings.SmtpPort,
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Confirma tu cuenta en ZonaAuto",
                    Body = body,
                    IsBodyHtml = true
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
