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

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ListaEmergenciasAdmin()
        {
            try
            {
                List<Cita> lista = await _citaRepositorio.ListaEmergenciasAdmin();

                var data = lista.Select(c => new
                {
                    idCita = c.IdCita, // ⚠️ Asegúrate de que se envíe como idCita
                    fechaCita = c.FechaCita,
                    horaCita = c.HoraCita,
                    paciente = c.Usuario?.Nombre ?? "Sin nombre",
                    especialidad = c.Especialidad?.Nombre ?? "Sin especialidad",
                    motivo = c.Indicaciones ?? "Sin motivo"
                }).ToList();

                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<object>(), error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GraficaEmergenciasAtendidas()
        {
            var lista = await _citaRepositorio.GraficaEmergenciasAtendidas();
            var data = lista.Select(x => new { fecha = x.Fecha, total = x.Total }).ToList();
            return Json(new { data = data });
        }

        [HttpPost]
        public async Task<IActionResult> AsignarEmergenciaAdmin(int idCita, int idDoctor)
        {
            try
            {
                string respuesta = await _citaRepositorio.AsignarEmergenciaAdmin(idCita, idDoctor);
                if (string.IsNullOrEmpty(respuesta))
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = respuesta });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GraficaEmergenciasRechazadas()
        {
            var lista = await _citaRepositorio.GraficaEmergenciasRechazadas();
            var data = lista.Select(x => new { fecha = x.Fecha, total = x.Total }).ToList();
            return Json(new { data = data });
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