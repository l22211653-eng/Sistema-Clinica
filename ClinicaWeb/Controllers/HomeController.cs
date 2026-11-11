using ClinicaData.Contrato; // ⚠️ NUEVO: Para usar ICitaRepositorio
using ClinicaEntidades;     // ⚠️ NUEVO: Para usar Cita
using ClinicaWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace ClinicaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICitaRepositorio _citaRepositorio; // ⚠️ NUEVO

        // ⚠️ Constructor actualizado para recibir el repositorio
        public HomeController(ILogger<HomeController> logger, ICitaRepositorio citaRepositorio)
        {
            _logger = logger;
            _citaRepositorio = citaRepositorio;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        // ⚠️ MÉTODO NUEVO: Para el panel de emergencias del Admin
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ListaEmergenciasAdmin()
        {
            try
            {
                List<Cita> lista = await _citaRepositorio.ListaEmergenciasAdmin();
                return Json(new { data = lista });
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<Cita>(), error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GraficaEmergenciasRechazadas()
        {
            var lista = await _citaRepositorio.GraficaEmergenciasRechazadas();
            return Json(new { data = lista });
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}