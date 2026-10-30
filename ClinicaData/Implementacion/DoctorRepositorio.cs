using ClinicaData.Contrato;
using ClinicaEntidades;
using ClinicaData.Configuracion;
using Microsoft.Extensions.Options;
using ClinicaEntidades.DTO;
using Npgsql;
using System.Text.Json;

namespace ClinicaData.Implementacion
{
    public class DoctorRepositorio : IDoctorRepositorio
    {
        private readonly ConnectionStrings con;
        public DoctorRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<string> Editar(Doctor objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_editarDoctor(@p_Id, @p_Doc, @p_Nom, @p_Ape, @p_Gen, @p_IdEsp, @p_FotoUrl)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", objeto.IdDoctor);
                    cmd.Parameters.AddWithValue("p_Doc", objeto.NumeroDocumentoIdentidad);
                    cmd.Parameters.AddWithValue("p_Nom", objeto.Nombres);
                    cmd.Parameters.AddWithValue("p_Ape", objeto.Apellidos);
                    cmd.Parameters.AddWithValue("p_Gen", objeto.Genero);
                    cmd.Parameters.AddWithValue("p_IdEsp", objeto.Especialidad.IdEspecialidad);
                    cmd.Parameters.AddWithValue("p_FotoUrl", (object?)objeto.FotoUrl ?? DBNull.Value);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al editar el doctor: " + ex.Message;
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
                using (var cmd = new NpgsqlCommand("SELECT sp_eliminarDoctor(@p_Id)", conexion))
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

        public async Task<string> EliminarHorario(int Id)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_eliminarDoctorHorario(@p_Id)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", Id);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al eliminar el horario: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<string> Guardar(Doctor objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_guardarDoctor(@p_Doc, @p_Nom, @p_Ape, @p_Gen, @p_IdEsp, @p_FotoUrl)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Doc", objeto.NumeroDocumentoIdentidad);
                    cmd.Parameters.AddWithValue("p_Nom", objeto.Nombres);
                    cmd.Parameters.AddWithValue("p_Ape", objeto.Apellidos);
                    cmd.Parameters.AddWithValue("p_Gen", objeto.Genero);
                    cmd.Parameters.AddWithValue("p_IdEsp", objeto.Especialidad.IdEspecialidad);
                    cmd.Parameters.AddWithValue("p_FotoUrl", (object?)objeto.FotoUrl ?? DBNull.Value);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al guardar el doctor: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<List<Doctor>> Lista()
        {
            List<Doctor> lista = new List<Doctor>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaDoctor()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Doctor()
                            {
                                FotoUrl = dr["FotoUrl"] != DBNull.Value ? dr["FotoUrl"].ToString()! : null,
                                IdDoctor = Convert.ToInt32(dr["IdDoctor"]),
                                NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                                Nombres = dr["Nombres"].ToString()!,
                                Apellidos = dr["Apellidos"].ToString()!,
                                Genero = dr["Genero"].ToString()!,
                                Especialidad = new Especialidad()
                                {
                                    IdEspecialidad = Convert.ToInt32(dr["IdEspecialidad"]),
                                    Nombre = dr["NombreEspecialidad"].ToString()!,
                                },
                                FechaCreacion = dr["FechaCreacion"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<Cita>> ListaCitasAsignadas(int Id, int IdEstadoCita)
        {
            List<Cita> lista = new List<Cita>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_ListaCitasAsignadas(@p_Id, @p_IdEstado)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", Id);
                    cmd.Parameters.AddWithValue("p_IdEstado", IdEstadoCita);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Cita()
                            {
                                IdCita = Convert.ToInt32(dr["IdCita"]),
                                FechaCita = dr["FechaCita"].ToString()!,
                                HoraCita = dr["HoraCita"].ToString()!,
                                Usuario = new Usuario()
                                {
                                    Nombre = dr["Nombre"].ToString()!,
                                    Apellido = dr["Apellido"].ToString()!,
                                },
                                EstadoCita = new EstadoCita()
                                {
                                    Nombre = dr["EstadoCita"].ToString()!
                                },
                                Indicaciones = dr["Indicaciones"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<DoctorHorario>> ListaDoctorHorario()
        {
            List<DoctorHorario> lista = new List<DoctorHorario>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaDoctorHorario()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new DoctorHorario()
                            {
                                IdDoctorHorario = Convert.ToInt32(dr["IdDoctorHorario"]),
                                Doctor = new Doctor()
                                {
                                    // ⚠️ Validamos si IdDoctor viene nulo o no existe
                                    IdDoctor = dr["IdDoctor"] != DBNull.Value ? Convert.ToInt32(dr["IdDoctor"]) : 0,
                                    NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                                    Nombres = dr["Nombres"].ToString()!,
                                    Apellidos = dr["Apellidos"].ToString()!,
                                },
                                NumeroMes = Convert.ToInt32(dr["NumeroMes"]),
                                // ⚠️ Validamos si las horas vienen nulas (ahora son opcionales)
                                HoraInicioAM = dr["HoraInicioAM"] != DBNull.Value ? dr["HoraInicioAM"].ToString()! : "",
                                HoraFinAM = dr["HoraFinAM"] != DBNull.Value ? dr["HoraFinAM"].ToString()! : "",
                                HoraInicioPM = dr["HoraInicioPM"] != DBNull.Value ? dr["HoraInicioPM"].ToString()! : "",
                                HoraFinPM = dr["HoraFinPM"] != DBNull.Value ? dr["HoraFinPM"].ToString()! : "",
                                FechaCreacion = dr["FechaCreacion"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<FechaAtencionDTO>> ListaDoctorHorarioDetalle(int Id)
        {
            List<FechaAtencionDTO> lista = new List<FechaAtencionDTO>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaDoctorHorarioDetalle(@p_Id)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", Id);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var fecha = dr["Fecha"].ToString()!;
                            var jsonHorarios = dr["Horarios"].ToString()!;

                            var fechaDto = new FechaAtencionDTO
                            {
                                Fecha = fecha,
                                HorarioDTO = new List<HorarioDTO>()
                            };

                            if (!string.IsNullOrEmpty(jsonHorarios))
                            {
                                using (var doc = JsonDocument.Parse(jsonHorarios))
                                {
                                    foreach (var item in doc.RootElement.EnumerateArray())
                                    {
                                        fechaDto.HorarioDTO.Add(new HorarioDTO
                                        {
                                            IdDoctorHorarioDetalle = item.GetProperty("IdDoctorHorarioDetalle").GetInt32(),
                                            Turno = item.GetProperty("Turno").GetString()!,
                                            TurnoHora = item.GetProperty("TurnoHora").GetString()!
                                        });
                                    }
                                }
                            }

                            lista.Add(fechaDto);
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<string> RegistrarHorario(DoctorHorario objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_registrarDoctorHorario(@p_Id, @p_Mes, @p_HAM, @p_HFAM, @p_HPM, @p_HFPM, @p_Fechas)", conexion))
                {
                    // ⚠️ Validación: Si objeto.Doctor es nulo, usamos objeto.IdDoctor
                    int idDoctor = objeto.Doctor != null ? objeto.Doctor.IdDoctor : objeto.IdDoctor;
                    cmd.Parameters.AddWithValue("p_Id", idDoctor);

                    cmd.Parameters.AddWithValue("p_Mes", objeto.NumeroMes);
                    cmd.Parameters.AddWithValue("p_HAM", objeto.HoraInicioAM ?? "");
                    cmd.Parameters.AddWithValue("p_HFAM", objeto.HoraFinAM ?? "");
                    cmd.Parameters.AddWithValue("p_HPM", objeto.HoraInicioPM ?? "");
                    cmd.Parameters.AddWithValue("p_HFPM", objeto.HoraFinPM ?? "");
                    cmd.Parameters.AddWithValue("p_Fechas", objeto.DoctorHorarioDetalle.Fecha);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al registrar el horario: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<List<Doctor>> ListaDoctoresSinHorario()
        {
            List<Doctor> lista = new List<Doctor>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_DoctoresSinHorario30Dias()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Doctor()
                            {
                                IdDoctor = Convert.ToInt32(dr["IdDoctor"]),
                                Nombres = dr["Nombres"].ToString()!,
                                Apellidos = dr["Apellidos"].ToString()!,
                                Especialidad = new Especialidad()
                                {
                                    Nombre = dr["Especialidad"].ToString()!
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<RendimientoDoctor>> ListaRendimiento30Dias()
        {
            List<RendimientoDoctor> lista = new List<RendimientoDoctor>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_Rendimiento30Dias()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new RendimientoDoctor()
                            {
                                Especialidad = dr["Especialidad"].ToString()!,
                                Doctor = dr["Doctor"].ToString()!,
                                TotalAtendidas = Convert.ToInt32(dr["TotalAtendidas"])
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public async Task<string> EditarHorario(DoctorHorario objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_editarDoctorHorario(@p_Id, @p_IdDoctor, @p_HAM, @p_HFAM, @p_HPM, @p_HFPM)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Id", objeto.IdDoctorHorario);

                    // ⚠️ VALIDACIÓN CLAVE: Si Doctor es nulo, usamos objeto.IdDoctor
                    int idDoctor = objeto.Doctor != null ? objeto.Doctor.IdDoctor : objeto.IdDoctor;
                    cmd.Parameters.AddWithValue("p_IdDoctor", idDoctor);

                    cmd.Parameters.AddWithValue("p_HAM", objeto.HoraInicioAM ?? "");
                    cmd.Parameters.AddWithValue("p_HFAM", objeto.HoraFinAM ?? "");
                    cmd.Parameters.AddWithValue("p_HPM", objeto.HoraInicioPM ?? "");
                    cmd.Parameters.AddWithValue("p_HFPM", objeto.HoraFinPM ?? "");

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al editar: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<List<DoctorBajoRendimiento>> ListaDoctoresBajoRendimiento()
        {
            List<DoctorBajoRendimiento> lista = new List<DoctorBajoRendimiento>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_DoctoresBajoRendimiento()", conexion))
                {
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new DoctorBajoRendimiento()
                            {
                                Doctor = dr["Doctor"].ToString()!,
                                Especialidad = dr["Especialidad"].ToString()!,
                                TotalAtendidas = Convert.ToInt32(dr["TotalAtendidas"])
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}