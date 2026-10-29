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
        private readonly IUsuarioRepositorio _usuarioRepositorio; // ⚠️ NUEVO

        public DoctorController(IDoctorRepositorio doctorRepositorio, ICitaRepositorio citaRepositorio, IUsuarioRepositorio usuarioRepositorio)
        {
            _doctorRepositorio = doctorRepositorio;
            _citaRepositorio = citaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        // ⚠️ Función auxiliar que busca al doctor por el IdUsuario de la sesión
        private async Task<Doctor> ObtenerDoctorLogueado()
        {
            // 1. Obtenemos el ID de Usuario de los Claims
            ClaimsPrincipal claimuser = HttpContext.User;
            string idUsuarioStr = claimuser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault()!;

            if (string.IsNullOrEmpty(idUsuarioStr))
                return null!;

            int idUsuario = int.Parse(idUsuarioStr);

            // 2. Buscamos el usuario para obtener su documento
            var listaUsuarios = await _usuarioRepositorio.Lista(0);
            var usuario = listaUsuarios.FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return null!;

            // 3. Con el documento, buscamos al doctor
            var listaDoctores = await _doctorRepositorio.Lista();
            return listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuario.NumeroDocumentoIdentidad)!;
        }

        // ⚠️ Función auxiliar para cargar la especialidad
        private async Task CargarEspecialidad()
        {
            var doctor = await ObtenerDoctorLogueado();
            if (doctor != null && doctor.Especialidad != null)
                ViewBag.Especialidad = doctor.Especialidad.Nombre;
            else
                ViewBag.Especialidad = "Sin especialidad";
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

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CitasAsignadas()
        {
            await CargarEspecialidad();
            return View("~/Views/Citas/CitasAsignadas.cshtml");
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> HistorialDoctor()
        {
            await CargarEspecialidad();
            return View("~/Views/Citas/HistorialDoctor.cshtml");
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
        public async Task<IActionResult> ListaCitasAsignadas()
        {
            try
            {
                var doctor = await ObtenerDoctorLogueado();
                int idDoctor = doctor?.IdDoctor ?? 1;

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, 1);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListaCitasAtendidas(string orden = "recientes")
        {
            try
            {
                var doctor = await ObtenerDoctorLogueado();
                int idDoctor = doctor?.IdDoctor ?? 1;

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, 2);

                if (orden == "antiguos")
                    lista = lista.OrderBy(x => DateTime.ParseExact(x.FechaCita, "dd/MM/yyyy", null)).ToList();
                else
                    lista = lista.OrderByDescending(x => DateTime.ParseExact(x.FechaCita, "dd/MM/yyyy", null)).ToList();

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