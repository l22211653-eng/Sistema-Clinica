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
                                YaCalificado = Convert.ToBoolean(dr["YaCalificado"]),
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
                                    // Leemos el IdDoctor que nos devuelve la función de PostgreSQL
                                    IdDoctor = Convert.ToInt32(dr["IdDoctor"]),
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

        // ==========================================
        // MÉTODOS DE RESEÑAS
        // ==========================================
        public async Task<string> GuardarResena(Resena objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_guardarResena(@p_IdCita, @p_IdDoctor, @p_IdUsuario, @p_Puntuacion, @p_Comentario)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdCita", objeto.IdCita);
                    cmd.Parameters.AddWithValue("p_IdDoctor", objeto.IdDoctor);
                    cmd.Parameters.AddWithValue("p_IdUsuario", objeto.IdUsuario);
                    cmd.Parameters.AddWithValue("p_Puntuacion", objeto.Puntuacion);
                    cmd.Parameters.AddWithValue("p_Comentario", objeto.Comentario ?? "");
                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex) { respuesta = "Error: " + ex.Message; }
                }
            }
            return respuesta;
        }


        public async Task<List<Resena>> ListaResenasDoctor(int idDoctor)
        {
            List<Resena> lista = new List<Resena>();
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaResenasDoctor(@p_IdDoctor)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdDoctor", idDoctor);
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Resena()
                            {
                                IdResena = Convert.ToInt32(dr["IdResena"]),
                                Puntuacion = Convert.ToInt32(dr["Puntuacion"]),
                                Comentario = dr["Comentario"].ToString()!,
                                Paciente = dr["Paciente"].ToString()!,
                                Fecha = dr["Fecha"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // ==========================================
        // MÉTODOS DE RECETAS
        // ==========================================
        public async Task<string> GuardarReceta(Receta objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_guardarReceta(@p_IdCita, @p_Medicamento, @p_Dosis, @p_Indicaciones)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdCita", objeto.IdCita);
                    cmd.Parameters.AddWithValue("p_Medicamento", objeto.Medicamento);
                    cmd.Parameters.AddWithValue("p_Dosis", objeto.Dosis);
                    cmd.Parameters.AddWithValue("p_Indicaciones", objeto.Indicaciones ?? "");
                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex) { respuesta = "Error: " + ex.Message; }
                }
            }
            return respuesta;
        }


        public async Task<List<Receta>> ListaRecetasCita(int idCita)
        {
            List<Receta> lista = new List<Receta>();
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaRecetasCita(@p_IdCita)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdCita", idCita);
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Receta()
                            {
                                IdReceta = Convert.ToInt32(dr["IdReceta"]),
                                Medicamento = dr["Medicamento"].ToString()!,
                                Dosis = dr["Dosis"].ToString()!,
                                Indicaciones = dr["Indicaciones"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}