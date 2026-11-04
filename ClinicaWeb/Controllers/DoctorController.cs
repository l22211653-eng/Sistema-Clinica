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
        private readonly IEspecialidadRepositorio _especialidadRepositorio;

        public DoctorController(IDoctorRepositorio doctorRepositorio, ICitaRepositorio citaRepositorio, IEspecialidadRepositorio especialidadRepositorio)
        {
            _doctorRepositorio = doctorRepositorio;
            _citaRepositorio = citaRepositorio;
            _especialidadRepositorio = especialidadRepositorio;
        }

        // ⚠️ MÉTODO AUXILIAR: Busca al doctor usando el IdUsuario del Claim
        private async Task<Doctor?> ObtenerDoctorLogueado()
        {
            // 1. Obtenemos el IdUsuario del Claim (esto es 100% confiable)
            ClaimsPrincipal claimuser = HttpContext.User;
            string idUsuarioStr = claimuser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault()!;

            if (string.IsNullOrEmpty(idUsuarioStr))
                return null;

            int idUsuario = int.Parse(idUsuarioStr);

            var listaDoctores = await _doctorRepositorio.Lista();

            // 2. ⚠️ LA CLAVE: Buscamos al doctor usando el IdUsuario
            // Asumiendo que el IdUsuario del doctor es el mismo que el IdDoctor
            var doctor = listaDoctores.FirstOrDefault(d => d.IdDoctor == idUsuario);

            // 3. Si no coincide, intentamos por el documento (por si acaso)
            if (doctor == null)
            {
                var usuarioDoc = User.Identity?.Name?.Trim();
                doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioDoc);
            }

            return doctor;
        }

        // 1. Vista de Citas Pendientes
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CitasAsignadas()
        {
            try
            {
                var doctor = await ObtenerDoctorLogueado();

                if (doctor != null)
                    ViewBag.Especialidad = doctor.Especialidad.Nombre;
                else
                    ViewBag.Especialidad = "Sin especialidad";

                return View("~/Views/Citas/CitasAsignadas.cshtml");
            }
            catch (Exception)
            {
                return View("~/Views/Citas/CitasAsignadas.cshtml");
            }
        }

        // 3. Endpoint para cargar las Pendientes (Estado 1)
        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas()
        {
            try
            {
                // ⚠️ PRUEBA DEFINITIVA: Forzamos el ID a 1
                int idDoctor = 1;

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
                int idDoctor = 1; // ⚠️ PRUEBA DEFINITIVA

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, 2);

                if (orden == "antiguos")
                    lista = lista.OrderBy(x => DateTime.ParseExact(x.FechaCita, "dd/MM/yyyy", null)).ToList();
                else
                    lista = lista.OrderByDescending(x => DateTime.ParseExact(x.FechaCita, "dd/MM/yyyy", null)).ToList();

                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = "Error C#: " + ex.Message });
            }
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

        [HttpGet]
        public async Task<IActionResult> ObtenerEspecialidad()
        {
            var doctor = await ObtenerDoctorLogueado();
            if (doctor == null) return Content("");

            var listaEspecialidades = await _especialidadRepositorio.Lista();
            var especialidad = listaEspecialidades.FirstOrDefault(e => e.IdEspecialidad == doctor.Especialidad.IdEspecialidad);
            return Content(especialidad?.Nombre ?? "");
        }

        // 2. Vista del Historial de Citas Atendidas
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> HistorialDoctor()
        {
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

        [HttpPost]
        public async Task<IActionResult> DescartarEmergencia(int idCita)
        {
            try
            {
                string respuesta = await _citaRepositorio.CambiarEstado(idCita, 3, "Emergencia atendida en consultorio por el doctor.");
                return Json(new { success = true, message = "Descartada" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListaEmergencias()
        {
            try
            {
                var doctor = await ObtenerDoctorLogueado();
                if (doctor == null)
                    return StatusCode(500, new { data = new List<Cita>(), error = "No se encontró al doctor." });

                List<Cita> lista = await _citaRepositorio.ListaEmergenciasDoctor(doctor.IdDoctor);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
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