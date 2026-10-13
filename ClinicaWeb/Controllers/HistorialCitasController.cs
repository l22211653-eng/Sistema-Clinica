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
    }
}
