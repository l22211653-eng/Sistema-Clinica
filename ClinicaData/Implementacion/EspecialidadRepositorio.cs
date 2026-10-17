using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ClinicaData.Implementacion
{
    public class EspecialidadRepositorio : IEspecialidadRepositorio
    {
        private readonly ConnectionStrings con;
        public EspecialidadRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<string> Editar(Especialidad objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_editarEspecialidad(@p_Id, @p_Nombre)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", objeto.IdEspecialidad);
                    cmd.Parameters.AddWithValue("p_Nombre", objeto.Nombre);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al editar la especialidad: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<int> Eliminar(int Id)
        {
            int respuesta = 1;
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_eliminarEspecialidad(@p_Id)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", Id);
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        respuesta = 0;
                    }
                }
            }
            return respuesta;
        }

        public async Task<string> Guardar(Especialidad objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                // ⚠️ AQUÍ ESTÁ EL CAMBIO: se agregó ::varchar al parámetro
                using (var cmd = new NpgsqlCommand("SELECT sp_guardarEspecialidad(@p_Nombre::varchar)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Nombre", objeto.Nombre);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al guardar la especialidad: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<List<Especialidad>> Lista()
        {
            List<Especialidad> lista = new List<Especialidad>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaEspecialidad()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Especialidad()
                            {
                                IdEspecialidad = Convert.ToInt32(dr["IdEspecialidad"]),
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