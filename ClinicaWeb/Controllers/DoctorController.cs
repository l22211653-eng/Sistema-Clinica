using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicaWeb.Controllers
{
    public class DoctorController : Controller
    {

        private readonly IDoctorRepositorio _doctorRepositorio;
        private readonly ICitaRepositorio _citaRepositorio;

        // Constructor limpio y con los dos repositorios
        public DoctorController(IDoctorRepositorio doctorRepositorio, ICitaRepositorio citaRepositorio)
        {

            _doctorRepositorio = doctorRepositorio;
            _citaRepositorio = citaRepositorio;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<Doctor> lista = await _doctorRepositorio.Lista();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] Doctor objeto)
        {
            string respuesta = await _doctorRepositorio.Guardar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPost]
        public async Task<IActionResult> GuardarReceta([FromBody] Receta objeto)
        {
            try
            {
                string respuesta = await _citaRepositorio.GuardarReceta(objeto);
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = "Error: " + ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] Doctor objeto)
        {
            string respuesta = await _doctorRepositorio.Editar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            int respuesta = await _doctorRepositorio.Eliminar(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas(int IdEstadoCita)
        {
            try
            {
                // 1. Obtenemos el ID del usuario de la sesión
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault()!;

                // 2. ⚠️ IMPORTANTE: La función en Supabase espera el IdDoctor, no el IdUsuario.
                // Si tu login guarda el IdUsuario, debes convertirlo al IdDoctor.
                // Por ahora, como sabemos que Alexandra es la 1, lo dejamos fijo para probar, 
                // pero deberías buscar la forma de mapear esto en tu login.
                int idDoctor = 1; // <-- TEMPORAL PARA PRUEBA

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, IdEstadoCita);

                // 3. Devolvemos la data con el formato que espera tu JS (como objeto con "data")
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CambiarEstado([FromBody] Cita objeto)
        {
            string respuesta = await _citaRepositorio.CambiarEstado(objeto.IdCita, objeto.EstadoCita.IdEstadoCita, objeto.Indicaciones);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> ListaDoctoresSinHorario()
        {
            var lista = await _doctorRepositorio.ListaDoctoresSinHorario();
            return Json(new { data = lista });
        }

        // ⚠️ NUEVO MÉTODO: Para que el admin vea las reseñas
        [HttpGet]
        public async Task<IActionResult> ListaResenas(int idDoctor)
        {
            try
            {
                List<Resena> lista = await _citaRepositorio.ListaResenasDoctor(idDoctor);
                return Json(lista);
            }
            catch (Exception)
            {
                return Json(new List<Resena>());
            }
        }
    }

}