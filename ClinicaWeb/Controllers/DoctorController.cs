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
                // Obtenemos el valor del Claim (puede ser documento o nombre)
                string usuarioLogueado = User.Identity?.Name?.Trim() ?? "";

                if (string.IsNullOrEmpty(usuarioLogueado))
                    return RedirectToAction("Login", "Acceso");

                var listaDoctores = await _doctorRepositorio.Lista();

                // Buscamos al doctor por Documento o por Nombre Completo
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioLogueado)
                          ?? listaDoctores.FirstOrDefault(d => $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioLogueado, StringComparison.OrdinalIgnoreCase));

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
            string usuarioLogueado = User.Identity?.Name?.Trim() ?? "";
            if (string.IsNullOrEmpty(usuarioLogueado)) return Content("");

            var listaDoctores = await _doctorRepositorio.Lista();
            var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioLogueado)
                      ?? listaDoctores.FirstOrDefault(d => $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioLogueado, StringComparison.OrdinalIgnoreCase));

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
                string usuarioLogueado = User.Identity?.Name?.Trim() ?? "";
                var listaDoctores = await _doctorRepositorio.Lista();
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioLogueado)
                          ?? listaDoctores.FirstOrDefault(d => $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioLogueado, StringComparison.OrdinalIgnoreCase));

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

        // 3. Endpoint para cargar las Pendientes (Estado 1)
        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas()
        {
            try
            {
                string usuarioLogueado = User.Identity?.Name?.Trim() ?? "";
                if (string.IsNullOrEmpty(usuarioLogueado))
                    return StatusCode(401, new { data = new List<Cita>(), error = "No hay sesión activa." });

                var listaDoctores = await _doctorRepositorio.Lista();

                // Buscamos al doctor por Documento o Nombre
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioLogueado)
                          ?? listaDoctores.FirstOrDefault(d => $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioLogueado, StringComparison.OrdinalIgnoreCase));

                if (doctor == null)
                    return StatusCode(500, new { data = new List<Cita>(), error = $"No se encontró al doctor: '{usuarioLogueado}'" });

                List<Cita> lista = await _doctorRepositorio.ListaCitasAsignadas(doctor.IdDoctor, 1);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }

        // 4. Endpoint para cargar las Atendidas (Estado 2)
        [HttpGet]
        public async Task<IActionResult> ListaCitasAtendidas(string orden = "recientes")
        {
            try
            {
                string usuarioLogueado = User.Identity?.Name?.Trim() ?? "";
                if (string.IsNullOrEmpty(usuarioLogueado))
                    return StatusCode(401, new { data = new List<Cita>(), error = "No hay sesión activa." });

                var listaDoctores = await _doctorRepositorio.Lista();

                // Buscamos al doctor por Documento o Nombre
                var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == usuarioLogueado)
                          ?? listaDoctores.FirstOrDefault(d => $"{d.Nombres} {d.Apellidos}".Trim().Equals(usuarioLogueado, StringComparison.OrdinalIgnoreCase));

                if (doctor == null)
                    return StatusCode(500, new { data = new List<Cita>(), error = $"No se encontró al doctor: '{usuarioLogueado}'" });

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