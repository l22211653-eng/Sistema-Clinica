using ClinicaEntidades;

namespace ClinicaData.Contrato
{
    public interface ICitaRepositorio
    {
        // Métodos de Citas
        Task<string> Guardar(Cita objeto);
        Task<string> Cancelar(int Id);
        Task<List<Cita>> ListaCitasPendiente(int IdUsuario);
        Task<List<Cita>> ListaHistorialCitas(int IdUsuario);
        Task<string> CambiarEstado(int IdCita, int IdEstado, string Indicaciones);

        // Métodos de Reseñas
        Task<List<Resena>> ListaResenasDoctor(int IdDoctor);
        Task<string> GuardarResena(Resena objeto);

        // Métodos de Emergencias
        Task LiberarTurno(int idCita);
        Task<string> CrearEmergencia(int idUsuario, int idEspecialidad, string motivo);
        Task<List<Cita>> ListaEmergenciasDoctor(int idDoctor);

        // ⚠️ NUEVO MÉTODO
        Task<string> RechazarEmergencia(int idCita, int idDoctor);

        // Métodos de Recetas
        Task<List<Receta>> ListaRecetasCita(int IdCita);
        Task<string> GuardarReceta(Receta objeto);
    }
}