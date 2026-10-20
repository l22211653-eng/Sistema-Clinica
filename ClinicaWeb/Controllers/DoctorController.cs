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
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault()!;

                // 1. Buscamos el documento del usuario logueado para identificar al doctor
                // Como en tu login se guarda el documento en el Claim, o el Identity.Name
                var usuarioDoc = User.Identity?.Name;

                // 2. Buscamos en la lista de doctores quién tiene ese documento
                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioDoc);

                // 3. Si lo encontramos, usamos su ID. Si no, usamos 1 por defecto (para pruebas)
                int idDoctor = doctor?.IdDoctor ?? 1;

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, IdEstadoCita);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListaCitasAtendidas(int IdEstadoCita)
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault()!;

                // ⚠️ IMPORTANTE: Por ahora usamos el ID 1 fijo para que pruebes con Alexandra, 
                // igual que hicimos en CitasAsignadas.
                int idDoctor = 1;

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, IdEstadoCita);
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