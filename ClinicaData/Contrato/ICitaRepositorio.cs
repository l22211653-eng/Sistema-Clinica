
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

        Task<string> CrearEmergencia(int idUsuario, int idEspecialidad);
        Task<List<Cita>> ListaEmergenciasDoctor(int idDoctor);

        // Métodos de Recetas
        Task<List<Receta>> ListaRecetasCita(int IdCita);
        Task<string> GuardarReceta(Receta objeto);

    }
}
