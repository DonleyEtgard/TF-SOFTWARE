using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ZONAUTO.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ZonautoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

// 3️⃣ Sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4️⃣ Autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/UserLogins/Login";
        options.LogoutPath = "/UserLogins/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// 5️⃣ Para usar HttpContext en vistas
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 6️⃣ Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Orden correcto
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 7️⃣ Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
