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

        // ⚠️ MÉTODO AUXILIAR: Obtiene el doctor logueado
        private async Task<Doctor?> ObtenerDoctorLogueado()
        {
            // 1. Leer del Cookie
            string? documentoDoc = HttpContext.Request.Cookies["DoctorDocumento"];

            // 2. Si no está, leer del Claim
            if (string.IsNullOrEmpty(documentoDoc))
            {
                documentoDoc = User.Identity?.Name?.Trim();
            }

            // ⚠️ LOGS PARA DEPURAR
            Console.WriteLine("=== DEPURANDO DOCTOR ===");
            Console.WriteLine("Documento leído: " + (documentoDoc ?? "NULL"));
            Console.WriteLine("Cookie: " + (HttpContext.Request.Cookies["DoctorDocumento"] ?? "NULL"));
            Console.WriteLine("User.Identity.Name: " + (User.Identity?.Name ?? "NULL"));

            if (string.IsNullOrEmpty(documentoDoc))
            {
                Console.WriteLine("No se encontró documento.");
                return null;
            }

            var listaDoctores = await _doctorRepositorio.Lista();
            var doctor = listaDoctores.FirstOrDefault(d => d.NumeroDocumentoIdentidad == documentoDoc);

            if (doctor == null)
            {
                Console.WriteLine("No se encontró doctor con ese documento. Documentos en BD: " + string.Join(", ", listaDoctores.Select(d => d.NumeroDocumentoIdentidad)));
            }
            else
            {
                Console.WriteLine("Doctor encontrado: " + doctor.Nombres + " " + doctor.Apellidos + " (ID: " + doctor.IdDoctor + ")");
            }

            return doctor;
        }

        // 1. Vista de Citas Pendientes
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CitasAsignadas()
        {
            var doctor = await ObtenerDoctorLogueado();
            ViewBag.Especialidad = doctor != null ? doctor.Especialidad.Nombre : "Sin especialidad";
            return View("~/Views/Citas/CitasAsignadas.cshtml");
        }

        // 2. Endpoint para cargar la tabla de Citas Pendientes
        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas()
        {
            try
            {
                var doctor = await ObtenerDoctorLogueado();
                int idDoctor = doctor?.IdDoctor ?? 1;

                // ⚠️ Traemos las Pendientes (1) y las Aceptadas (4)
                List<Cita> listaPendientes = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, 1);
                List<Cita> listaAceptadas = await _doctorRepositorio.ListaCitasAsignadas(idDoctor, 4);

                // Unimos ambas listas
                List<Cita> listaCompleta = listaPendientes.Concat(listaAceptadas).ToList();

                return StatusCode(StatusCodes.Status200OK, new { data = listaCompleta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }

        // 3. Endpoint para cargar el Historial de Atendidas
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

        // 4. Endpoint para listar emergencias
        [HttpGet]
        public async Task<IActionResult> ListaEmergencias()
        {
            try
            {
                var doctor = await ObtenerDoctorLogueado();
                int idDoctor = doctor?.IdDoctor ?? 1;

                List<Cita> lista = await _citaRepositorio.ListaEmergenciasDoctor(idDoctor);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { data = new List<Cita>(), error = ex.Message });
            }
        }

        // 5. Método para Aceptar Emergencia
        [HttpPost]
        public async Task<IActionResult> AceptarEmergencia(int idCita)
        {
            try
            {
                // ⚠️ Cambiamos al estado "Aceptada" (ID 4)
                // Si en tu base de datos el ID es otro, cámbialo aquí.
                string respuesta = await _citaRepositorio.CambiarEstado(idCita, 4, "");
                return Json(new { data = respuesta });
            }
            catch (Exception ex)
            {
                return Json(new { data = "Error: " + ex.Message });
            }
        }

        // 6. Método para Descartar Emergencia
        [HttpPost]
        public async Task<IActionResult> DescartarEmergencia(int idCita)
        {
            try
            {
                string respuesta = await _citaRepositorio.CambiarEstado(idCita, 3, "Emergencia descartada.");
                return Json(new { success = true, message = "Descartada" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 7. Vista del Historial
        [Authorize(Roles = "Doctor")]
        public IActionResult HistorialDoctor()
        {
            return View("~/Views/Citas/HistorialDoctor.cshtml");
        }

        // ... (El resto de métodos como Index, Lista, Guardar, Editar, Eliminar, etc. se mantienen igual)
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