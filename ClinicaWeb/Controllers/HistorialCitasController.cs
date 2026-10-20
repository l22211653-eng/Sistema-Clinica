using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicaWeb.Controllers
{
    public class HistorialCitasController : Controller
    {
        private readonly ICitaRepositorio _repositorioCita;
        private readonly ILogger<HistorialCitasController> _logger;

        public HistorialCitasController(ICitaRepositorio repositorioCita, ILogger<HistorialCitasController> logger)
        {
            _repositorioCita = repositorioCita;
            _logger = logger;
        }

        [Authorize(Roles = "Paciente")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaHistorialCitas()
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuario = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .SingleOrDefault()!;

                if (string.IsNullOrEmpty(idUsuario))
                {
                    return StatusCode(401, new { error = "No hay sesión activa. Vuelve a iniciar sesión." });
                }

                List<Cita> lista = await _repositorioCita.ListaHistorialCitas(int.Parse(idUsuario));

                return Json(lista ?? new List<Cita>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR CRÍTICO al listar historial de citas");
                return StatusCode(500, new { error = "Error interno del servidor: " + ex.Message });
            }
        }

        // ⚠️ NUEVO MÉTODO: Para cancelar/eliminar la cita desde el historial
        [HttpDelete]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                string respuesta = await _repositorioCita.Cancelar(id);
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = "Error en el servidor: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GuardarResena([FromBody] Resena objeto)
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuario = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .SingleOrDefault()!;

                if (!string.IsNullOrEmpty(idUsuario))
                {
                    objeto.IdUsuario = int.Parse(idUsuario);
                }

                string respuesta = await _repositorioCita.GuardarResena(objeto);

                if (string.IsNullOrEmpty(respuesta))
                    return Json(new { success = true, message = "Reseña guardada" });
                else
                    return Json(new { success = false, message = respuesta });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListaRecetasCita(int idCita)
        {
            try
            {
                List<Receta> lista = await _repositorioCita.ListaRecetasCita(idCita);
                return Json(lista);
            }
            catch (Exception ex)
            {
                return Json(new List<Receta>());
            }
        }
    }
}