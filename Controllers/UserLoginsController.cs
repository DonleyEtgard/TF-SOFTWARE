using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ZONAUTO.Controllers
{
    public class UserLoginsController : Controller
    {
        private readonly ZonautoContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserLoginsController> _logger;

        public UserLoginsController(
            ZonautoContext context,
            IConfiguration configuration,
            ILogger<UserLoginsController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        #region Authentication Methods

        [HttpGet]
        public IActionResult Login()
        {
            ClearSession();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email y contraseña son obligatorios.";
                return View();
            }

            email = email.Trim().ToLower();

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.Persona.Email.ToLower() == email);

            if (usuario == null || !usuario.Habilitado || !BCrypt.Net.BCrypt.Verify(password, usuario.ContrasenaHash))
            {
                // Registrar intento fallido
                _context.UserLogins.Add(new UserLogin
                {
                    UsuarioId = usuario?.UsuarioId,
                    FechaLogin = DateTime.Now,
                    DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    FueExitoso = false
                });
                await _context.SaveChangesAsync();

                TempData["Error"] = "Credenciales inválidas o cuenta deshabilitada.";
                return View();
            }

            // Registrar login exitoso
            _context.UserLogins.Add(new UserLogin
            {
                UsuarioId = usuario.UsuarioId,
                FechaLogin = DateTime.Now,
                DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                FueExitoso = true
            });
            await _context.SaveChangesAsync();

            // Configurar sesión y claims
            HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
            HttpContext.Session.SetString("Email", usuario.Persona.Email);
            HttpContext.Session.SetString("NombreCompleto", $"{usuario.Persona.Nombre} {usuario.Persona.Apellido}");
            HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario.Nombre);
            HttpContext.Session.SetString("EstaVerificado", usuario.Verificado.ToString());

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Persona.Nombre),
        new Claim(ClaimTypes.Email, usuario.Persona.Email),
        new Claim(ClaimTypes.Role, usuario.TipoUsuario.Nombre),
        new Claim("Verificado", usuario.Verificado.ToString())
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // Redirigir según tipo de usuario
            return usuario.TipoUsuario.Nombre switch
            {
                "Administrador" => RedirectToAction("Dashboard", "Administradores"),
                "Comprador" => RedirectToAction("Index", "Publicaciones"),
                "Vendedor" => RedirectToAction("Index", "Ofertas"),
                _ => RedirectToAction("Index", "Home", new { autenticado = true })
            };
        }


        [HttpGet]
        public async Task<IActionResult> ReenviarConfirmacion(string email)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Persona.Email == email);

            if (usuario == null)
            {
                TempData["Error"] = "No se encontró el usuario";
                return RedirectToLogin();
            }

            usuario.TokenVerificacion ??= Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();

            await EnviarEmailVerificacion(usuario.Persona.Email, usuario.Persona.Nombre, usuario.TokenVerificacion);

            TempData["Success"] = "Se ha enviado un nuevo correo de verificación";
            return RedirectToLogin();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmarCuenta(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "Token de verificación inválido";
                return RedirectToLogin();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.TokenVerificacion == token);

            if (usuario == null)
            {
                TempData["Error"] = "Token de verificación no válido o expirado";
                return RedirectToLogin();
            }

            usuario.Verificado = true;
            usuario.TokenVerificacion = null;
            await _context.SaveChangesAsync();

            await SesionDeVerificacion(usuario);

            TempData["Success"] = $"¡Cuenta verificada con éxito! Bienvenido {usuario.Persona.Nombre}";
            return RedirectToHome();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            ClearSession();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Sesión cerrada correctamente.";
            return RedirectToLogin();
        }

        #endregion

        #region CRUD Operations

        public async Task<IActionResult> Index()
        {
            var logins = await _context.UserLogins
                .Include(l => l.Usuario)
                .ThenInclude(u => u.Persona)
                .OrderByDescending(l => l.FechaLogin)
                .AsNoTracking()
                .ToListAsync();

            return View(logins);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var login = await _context.UserLogins
                .Include(l => l.Usuario)
                .ThenInclude(u => u.Persona)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.LoginId == id);

            return login == null ? NotFound() : View(login);
        }

        public IActionResult Create()
        {
            ViewData["UsuarioId"] = GetUsuariosSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsuarioId,FechaLogin,FueExitoso,DireccionIp")] UserLogin userLogin)
        {
            if (!ModelState.IsValid)
            {
                ViewData["UsuarioId"] = GetUsuariosSelectList(userLogin.UsuarioId);
                return View(userLogin);
            }

            _context.Add(userLogin);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var login = await _context.UserLogins.FindAsync(id);
            if (login == null)
                return NotFound();

            ViewData["UsuarioId"] = GetUsuariosSelectList(login.UsuarioId);
            return View(login);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LoginId,UsuarioId,FechaLogin,FueExitoso,DireccionIp")] UserLogin login)
        {
            if (id != login.LoginId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["UsuarioId"] = GetUsuariosSelectList(login.UsuarioId);
                return View(login);
            }

            try
            {
                _context.Update(login);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserLoginExists(login.LoginId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var login = await _context.UserLogins
                .Include(l => l.Usuario)
                .ThenInclude(u => u.Persona)
                .FirstOrDefaultAsync(m => m.LoginId == id);

            return login == null ? NotFound() : View(login);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var login = await _context.UserLogins.FindAsync(id);
            if (login != null)
            {
                _context.UserLogins.Remove(login);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Private Helper Methods

        private void ClearSession()
        {
            HttpContext.Session.Clear();
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home", new { autenticado = true });
        }

        private async Task<bool> UserLoginExists(int id)
        {
            return await _context.UserLogins.AnyAsync(e => e.LoginId == id);
        }

        private bool ValidateLoginInput(string email, string password, out IActionResult result)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email y contraseña son obligatorios.";
                result = View();
                return false;
            }
            result = null;
            return true;
        }

        private bool ValidateUsuario(Usuario usuario, string password, string email, out IActionResult result)
        {
            if (usuario == null || !usuario.Habilitado || !BCrypt.Net.BCrypt.Verify(password, usuario.ContrasenaHash))
            {
                RegistrarIntentoFallido(usuario?.UsuarioId, email).Wait();
                TempData["Error"] = "Credenciales inválidas o cuenta deshabilitada.";
                result = View();
                return false;
            }
            result = null;
            return true;
        }

        private async Task RegistrarLoginExitos(Usuario usuario)
        {
            _context.UserLogins.Add(new UserLogin
            {
                UsuarioId = usuario.UsuarioId,
                FechaLogin = DateTime.Now,
                DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                FueExitoso = true
            });
            await _context.SaveChangesAsync();
        }

        private async Task SetUserSessionAndClaims(Usuario usuario)
        {
            HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
            HttpContext.Session.SetString("Email", usuario.Persona.Email);
            HttpContext.Session.SetString("NombreCompleto", $"{usuario.Persona.Nombre} {usuario.Persona.Apellido}");
            HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario.Nombre);
            HttpContext.Session.SetString("EstaVerificado", usuario.Verificado.ToString());

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Persona.Nombre),
                new Claim(ClaimTypes.Email, usuario.Persona.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.Nombre),
                new Claim("Verificado", usuario.Verificado.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private void SetVerificationStatusMessage(Usuario usuario)
        {
            if (!usuario.Verificado)
            {
                TempData["Warning"] = $"Bienvenido {usuario.Persona.Nombre}. Tu cuenta no está verificada. " +
                                    $"<a href='{Url.Action("ReenviarConfirmacion", "UserLogins", new { email = usuario.Persona.Email })}'>" +
                                    "¿Reenviar correo de verificación?</a>";
            }
            else
            {
                TempData["Success"] = $"Bienvenido, {usuario.Persona.Nombre}";
            }
        }
             
        private IActionResult RedirectByUserType(string tipoUsuario)
        {
            return tipoUsuario switch
            {
                "Administrador" => RedirectToAction("Dashboard", "Administradores"),
                "Comprador" => RedirectToAction("Details", "Publicaciones"),
                "Vendedor" => RedirectToAction("Index", "Publicaciones"),
                _ => RedirectToHome()
            };
        }

        private async Task SesionDeVerificacion(Usuario usuario)
        {
            if (HttpContext.Session.GetInt32("UsuarioId") == usuario.UsuarioId)
            {
                HttpContext.Session.SetString("EstaVerificado", "True");

                await HttpContext.SignOutAsync();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Persona.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Persona.Email),
                    new Claim(ClaimTypes.Role, usuario.TipoUsuario.Nombre),
                    new Claim("Verificado", "True")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            }
        }

        private SelectList GetUsuariosSelectList(int? selectedUsuarioId = null)
        {
            return new SelectList(
                _context.Usuarios
                    .Include(u => u.Persona)
                    .Select(u => new
                    {
                        u.UsuarioId,
                        Email = u.Persona != null ? u.Persona.Email : "(Sin email)"
                    })
                    .AsNoTracking(),
                "UsuarioId",
                "Email",
                selectedUsuarioId);
        }

        private async Task EnviarEmailVerificacion(string email, string nombre, string token)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var urlConfirmacion = Url.Action("ConfirmarCuenta", "UserLogins", new { token }, Request.Scheme);

                using var message = new MailMessage
                {
                    From = new MailAddress(emailSettings["FromAddress"], emailSettings["FromName"]),
                    To = { new MailAddress(email, nombre) },
                    Subject = "Verificación de cuenta en ZonaAuto",
                    Body = $@"
                    <html>
                    <body>
                        <h2>Hola {nombre},</h2>
                        <p>Gracias por registrarte en ZonaAuto. Para verificar tu cuenta (opcional), haz clic en el siguiente enlace:</p>
                        <p><a href='{urlConfirmacion}'>Verificar Cuenta</a></p>
                        <p>Si no solicitaste este registro, puedes ignorar este mensaje.</p>
                        <p><strong>Nota:</strong> La verificación es opcional pero recomendada.</p>
                    </body>
                    </html>",
                    IsBodyHtml = true
                };

                using var smtp = new SmtpClient
                {
                    Host = emailSettings["SmtpServer"],
                    Port = int.Parse(emailSettings["Port"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"])
                };

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de confirmación");
            }
        }

        private async Task RegistrarIntentoFallido(int? usuarioId, string email)
        {
            _context.UserLogins.Add(new UserLogin
            {
                UsuarioId = usuarioId,
                FechaLogin = DateTime.Now,
                DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                FueExitoso = false
            });

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
