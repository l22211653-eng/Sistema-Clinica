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

        private async Task CargarEspecialidad()
        {
            // ⚠️ Volvemos a usar el documento
            string documentoLogueado = User.Identity?.Name ?? "";

            if (string.IsNullOrEmpty(documentoLogueado))
            {
                ViewBag.Especialidad = "Sin sesión";
                return;
            }

            var listaDoctores = await _doctorRepositorio.Lista();
            var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == documentoLogueado);

            if (doctor == null)
            {
                ViewBag.Especialidad = "Doctor no encontrado";
                return;
            }

            var listaEspecialidades = await _especialidadRepositorio.Lista();
            var especialidad = listaEspecialidades.FirstOrDefault(e => e.IdEspecialidad == doctor.Especialidad.IdEspecialidad);

            ViewBag.Especialidad = especialidad?.Nombre ?? "Sin especialidad";
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

        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas()
        {
            try
            {
                var usuarioNombre = User.Identity?.Name?.Trim();
                if (string.IsNullOrEmpty(usuarioNombre))
                    return StatusCode(401, new { data = new List<Cita>(), error = "No hay sesión activa." });

                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d =>
                    $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioNombre, StringComparison.OrdinalIgnoreCase));

                if (doctor == null)
                    doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioNombre);

                if (doctor == null)
                    return StatusCode(500, new { data = new List<Cita>(), error = $"No se encontró al doctor: '{usuarioNombre}'" });

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(doctor.IdDoctor, 1);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = "Error C#: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListaCitasAtendidas(string orden = "recientes")
        {
            try
            {
                var usuarioNombre = User.Identity?.Name?.Trim();
                if (string.IsNullOrEmpty(usuarioNombre))
                    return StatusCode(401, new { data = new List<Cita>(), error = "No hay sesión activa." });

                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d =>
                    $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioNombre, StringComparison.OrdinalIgnoreCase));

                if (doctor == null)
                    doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioNombre);

                if (doctor == null)
                    return StatusCode(500, new { data = new List<Cita>(), error = $"No se encontró al doctor: '{usuarioNombre}'" });

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(doctor.IdDoctor, 2);

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