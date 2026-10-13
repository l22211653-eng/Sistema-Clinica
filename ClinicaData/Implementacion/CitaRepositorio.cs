using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ClinicaData.Implementacion
{
    public class CitaRepositorio : ICitaRepositorio
    {
        private readonly ConnectionStrings con;
        public CitaRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<string> CambiarEstado(int IdCita, int IdEstado, string Indicaciones)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_CambiarEstadoCita(@p_IdCita, @p_IdEstado, @p_Indicaciones)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdCita", IdCita);
                    cmd.Parameters.AddWithValue("p_IdEstado", IdEstado);
                    // Manejo de nulos para el parámetro opcional
                    cmd.Parameters.AddWithValue("p_Indicaciones", (object)Indicaciones ?? DBNull.Value);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al cambiar estado: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<string> Cancelar(int Id)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_CancelarCita(@p_IdCita)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdCita", Id);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al cancelar la cita: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<string> Guardar(Cita objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_guardarCita(@p_IdUsuario, @p_IdDetalle, @p_IdEstado, @p_Fecha)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdUsuario", objeto.Usuario.IdUsuario);
                    cmd.Parameters.AddWithValue("p_IdDetalle", objeto.DoctorHorarioDetalle.IdDoctorHorarioDetalle);
                    cmd.Parameters.AddWithValue("p_IdEstado", objeto.EstadoCita.IdEstadoCita);
                    // Aseguramos que la fecha se envíe en el formato correcto DD/MM/YYYY
                    cmd.Parameters.AddWithValue("p_Fecha", objeto.FechaCita ?? DateTime.Now.ToString("dd/MM/yyyy"));

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al guardar la cita: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<List<Cita>> ListaCitasPendiente(int IdUsuario)
        {
            List<Cita> lista = new List<Cita>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_ListaCitasPendiente(@p_IdUsuario)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdUsuario", IdUsuario);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Cita()
                            {
                                IdCita = Convert.ToInt32(dr["IdCita"]),
                                FechaCita = dr["FechaCita"].ToString()!,
                                HoraCita = dr["HoraCita"].ToString()!,
                                Especialidad = new Especialidad()
                                {
                                    Nombre = dr["NombreEspecialidad"].ToString()!,
                                },
                                Doctor = new Doctor()
                                {
                                    Nombres = dr["Nombres"].ToString()!,
                                    Apellidos = dr["Apellidos"].ToString()!,
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<Cita>> ListaHistorialCitas(int IdUsuario)
        {
            List<Cita> lista = new List<Cita>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_ListaHistorialCitas(@p_IdUsuario)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdUsuario", IdUsuario);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Cita()
                            {
                                IdCita = Convert.ToInt32(dr["IdCita"]),
                                FechaCita = dr["FechaCita"].ToString()!,
                                HoraCita = dr["HoraCita"].ToString()!,
                                Indicaciones = dr["Indicaciones"].ToString()!,
                                Especialidad = new Especialidad()
                                {
                                    Nombre = dr["NombreEspecialidad"].ToString()!,
                                },
                                Doctor = new Doctor()
                                {
                                    Nombres = dr["Nombres"].ToString()!,
                                    Apellidos = dr["Apellidos"].ToString()!,
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}