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

        // 1. Vista de Citas Pendientes
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CitasAsignadas()
        {
            try
            {
                // Obtenemos el IdUsuario
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuarioStr = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .SingleOrDefault()!;

                if (string.IsNullOrEmpty(idUsuarioStr))
                    return RedirectToAction("Login", "Acceso");

                // Buscamos al doctor por su documento
                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == User.Identity?.Name);

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
            string documentoLogueado = User.Identity?.Name ?? "";
            if (string.IsNullOrEmpty(documentoLogueado)) return Content("");

            var listaDoctores = await _doctorRepositorio.Lista();
            var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == documentoLogueado);

            if (doctor != null)
            {
                var listaEspecialidades = await _especialidadRepositorio.Lista();
                var especialidad = listaEspecialidades.FirstOrDefault(e => e.IdEspecialidad == doctor.Especialidad.IdEspecialidad);
                return Content(especialidad?.Nombre ?? "");
            }
            return Content("");
        }

        // 2. Vista del Historial de Citas Atendidas
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> HistorialDoctor()
        {
            // Aquí también podrías cargar la especialidad si la necesitas en la vista
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
                var usuarioDoc = User.Identity?.Name;
                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioDoc);
                int idDoctor = doctor?.IdDoctor ?? 1;

                List<Cita> lista = await _citaRepositorio.ListaEmergenciasDoctor(idDoctor);
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

        // 2. Endpoint para cargar las Pendientes
        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas()
        {
            try
            {
                // Obtenemos el IdUsuario
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuarioStr = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .SingleOrDefault()!;

                if (string.IsNullOrEmpty(idUsuarioStr))
                    return StatusCode(401, new { data = new List<Cita>(), error = "No hay sesión activa." });

                int idUsuario = int.Parse(idUsuarioStr);

                // Buscamos al doctor por su documento (que está en el Name)
                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == User.Identity?.Name);

                // Si no lo encontramos, usamos el IdUsuario como fallback
                int idDoctor = doctor?.IdDoctor ?? idUsuario;

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, 1);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }

        // 3. Endpoint para cargar las Atendidas
        [HttpGet]
        public async Task<IActionResult> ListaCitasAtendidas(string orden = "recientes")
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                string idUsuarioStr = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .SingleOrDefault()!;

                if (string.IsNullOrEmpty(idUsuarioStr))
                    return StatusCode(401, new { data = new List<Cita>(), error = "No hay sesión activa." });

                int idUsuario = int.Parse(idUsuarioStr);

                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == User.Identity?.Name);

                int idDoctor = doctor?.IdDoctor ?? idUsuario;

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