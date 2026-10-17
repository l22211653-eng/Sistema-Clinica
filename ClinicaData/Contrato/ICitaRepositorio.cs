
using ClinicaEntidades;

namespace ClinicaData.Contrato
{
    public interface ICitaRepositorio
    {
        Task<string> Guardar(Cita objeto);
        Task<string> Cancelar(int Id);
        Task<List<Cita>> ListaCitasPendiente(int IdUsuario);
        Task<List<Cita>> ListaHistorialCitas(int IdUsuario);
        Task<string> CambiarEstado(int IdCita,int IdEstado,string Indicaciones);
        Task<List<Resena>> ListaResenas(int IdDoctor);
        Task<string> GuardarResena(Resena objeto);
        Task<string> CancelarResena();
        Task<List<Receta>> ListaRecetas(int IdCita);
        Task<string> GuardarReceta(Receta objeto);
        Task<string> CancelarRecetas();
        Task<List<Receta>> ListaRecetasCita(int IdCita);
    }
}
