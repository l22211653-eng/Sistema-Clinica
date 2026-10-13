using ClinicaData.Contrato;
using ClinicaEntidades;
using ClinicaData.Configuracion;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ClinicaData.Implementacion
{
    public class RolUsuarioRepositorio : IRolUsuarioRepositorio
    {
        private readonly ConnectionStrings con;
        public RolUsuarioRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<List<RolUsuario>> Lista()
        {
            List<RolUsuario> lista = new List<RolUsuario>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaRolUsuario()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new RolUsuario()
                            {
                                IdRolUsuario = Convert.ToInt32(dr["IdRolUsuario"]),
                                Nombre = dr["Nombre"].ToString()!,
                                FechaCreacion = dr["FechaCreacion"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}