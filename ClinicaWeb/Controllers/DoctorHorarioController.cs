using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpPost]
        public async Task<IActionResult> RegistrarHorario([FromBody] DoctorHorario objeto)
        {
            try
            {
                string respuesta = await _repositorio.RegistrarHorario(objeto);
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = "Error en el servidor: " + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            try
            {
                // Llamamos al repositorio. 
                // Si retorna "", se borró bien. Si retorna texto, es el error real.
                string respuesta = await _repositorio.EliminarHorario(Id);
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = "Error en el servidor: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListaDoctoresSinHorario()
        {
            var lista = await _repositorio.ListaDoctoresSinHorario();
            return Json(new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> ListaRendimiento30Dias()
        {
            var lista = await _repositorio.ListaRendimiento30Dias();
            return Json(new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> ListaDoctoresBajoRendimiento()
        {
            var lista = await _repositorio.ListaDoctoresBajoRendimiento();
            return Json(new { data = lista });
        }
    }
}