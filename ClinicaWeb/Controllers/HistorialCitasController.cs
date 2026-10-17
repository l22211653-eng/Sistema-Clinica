using ClinicaData.Contrato;
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
                    return StatusCode(StatusCodes.Status401Unauthorized, new { data = new List<Cita>(), error = "No se pudo obtener el usuario" });
                }

                List<Cita> lista = await _repositorioCita.ListaHistorialCitas(int.Parse(idUsuario));

                // Si la lista está vacía, devolvemos un array vacío (no null)
                return StatusCode(StatusCodes.Status200OK, new { data = lista ?? new List<Cita>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar historial de citas");
                // Devolvemos el error en JSON para que DataTables lo muestre legible
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    data = new List<Cita>(),
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        // 1. Método para guardar la reseña
        [HttpPost]
        public async Task<IActionResult> GuardarResena([FromBody] Resena objeto)
        {
            // Aquí deberías obtener el ID del usuario logueado desde la sesión o cookies
            // Por ahora, lo simulamos buscando en la sesión o poniendo un ID fijo para probar.
            // objeto.IdUsuario = ObtenerIdUsuarioLogueado(); 

            string respuesta = await _repositorioCita.GuardarResena(objeto);

            if (string.IsNullOrEmpty(respuesta))
                return Json(new { success = true, message = "Reseña guardada" });
            else
                return Json(new { success = false, message = respuesta });
        }

        // 2. Método para obtener las recetas de una cita
        [HttpGet]
        public async Task<IActionResult> ListaRecetasCita(int idCita)
        {
            List<Receta> lista = await _repositorioCita.ListaRecetasCita(idCita);
            return Json(lista);
        }
    }
}
