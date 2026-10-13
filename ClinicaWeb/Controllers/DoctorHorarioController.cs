using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaWeb.Controllers
{
    public class DoctorHorarioController : Controller
    {
        private readonly IDoctorRepositorio _repositorio;

        public DoctorHorarioController(IDoctorRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<DoctorHorario> lista = await _repositorio.ListaDoctorHorario();

            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> ListaDoctoresSinHorario()
        {
            var lista = await _repositorio.ListaDoctoresSinHorario();

            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> ListaRendimiento30Dias()
        {
            var lista = await _repositorio.ListaRendimiento30Dias();

            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> ListaDoctoresBajoRendimiento()
        {
            var lista = await _repositorio.ListaDoctoresBajoRendimiento();

            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        public async Task<IActionResult> Guardar([FromBody] DoctorHorario objeto)
        {
            string respuesta = await _repositorio.RegistrarHorario(objeto);

            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            string respuesta = await _repositorio.EliminarHorario(Id);

            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

    }
}