using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // ⚠️ Necesario para UseNpgsql y DbContext

var builder = WebApplication.CreateBuilder(args);

// Configurar el puerto dinámico de Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// ⚠️ FORZAR la lectura de la cadena de conexión desde variables de entorno
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CadenaSQL")
                       ?? builder.Configuration.GetConnectionString("CadenaSQL");

// Registrar manualmente la configuración con la cadena obtenida
builder.Services.Configure<ConnectionStrings>(options =>
{
    options.CadenaSQL = connectionString ?? throw new InvalidOperationException("La cadena de conexión 'CadenaSQL' no está configurada.");
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// ⚠️ CONFIGURACIÓN DE DATA PROTECTION
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<DataProtectionKeyContext>()
    .SetApplicationName("ClinicaWeb");

// Registrar el DbContext para las claves (necesita el paquete Npgsql.EntityFrameworkCore.PostgreSQL)
builder.Services.AddDbContext<DataProtectionKeyContext>(options =>
    options.UseNpgsql(connectionString));

// REGISTRAR EL SERVICIO DE CACHÉ (necesario para la sesión)
builder.Services.AddDistributedMemoryCache();

// REGISTRAR EL SERVICIO DE SESIÓN
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ICitaRepositorio, CitaRepositorio>();
builder.Services.AddScoped<IDoctorRepositorio, DoctorRepositorio>();
builder.Services.AddScoped<IEspecialidadRepositorio, EspecialidadRepositorio>();
builder.Services.AddScoped<IRolUsuarioRepositorio, RolUsuarioRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Acceso/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        option.AccessDeniedPath = "/Acceso/Denegado";
        option.Cookie.HttpOnly = true;
        option.Cookie.IsEssential = true;
        option.Cookie.SameSite = SameSiteMode.Lax;
        option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();

// ⚠️ CLASE NECESARIA: Debe implementar IDataProtectionKeyContext
public class DataProtectionKeyContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options) : base(options) { }

    // Esta propiedad mapea a la tabla "DataProtectionKeys" en Supabase
    public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; }
}