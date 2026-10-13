using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace ClinicaData.Implementacion
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ConnectionStrings con;
        public UsuarioRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<string> Editar(Usuario objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                // En PostgreSQL las funciones se llaman con SELECT * FROM
                using (var cmd = new NpgsqlCommand("SELECT sp_editarUsuario(@p_IdUsuario, @p_Doc, @p_Nom, @p_Ape, @p_Cor, @p_Cla, @p_Rol)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdUsuario", objeto.IdUsuario);
                    cmd.Parameters.AddWithValue("p_Doc", objeto.NumeroDocumentoIdentidad);
                    cmd.Parameters.AddWithValue("p_Nom", objeto.Nombre);
                    cmd.Parameters.AddWithValue("p_Ape", objeto.Apellido);
                    cmd.Parameters.AddWithValue("p_Cor", objeto.Correo);
                    cmd.Parameters.AddWithValue("p_Cla", objeto.Clave);
                    cmd.Parameters.AddWithValue("p_Rol", objeto.RolUsuario.IdRolUsuario);

                    try
                    {
                        // ExecuteScalar lee el TEXT que retorna la función
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al editar usuario: " + ex.Message;
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
                using (var cmd = new NpgsqlCommand("SELECT sp_eliminarUsuario(@p_IdUsuario)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdUsuario", Id);
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

        public async Task<string> Guardar(Usuario objeto)
        {
            string respuesta = "";
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT sp_guardarUsuario(@p_Doc, @p_Nom, @p_Ape, @p_Cor, @p_Cla, @p_Rol)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Doc", objeto.NumeroDocumentoIdentidad);
                    cmd.Parameters.AddWithValue("p_Nom", objeto.Nombre);
                    cmd.Parameters.AddWithValue("p_Ape", objeto.Apellido);
                    cmd.Parameters.AddWithValue("p_Cor", objeto.Correo);
                    cmd.Parameters.AddWithValue("p_Cla", objeto.Clave);
                    cmd.Parameters.AddWithValue("p_Rol", objeto.RolUsuario.IdRolUsuario);

                    try
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        respuesta = result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        respuesta = "Error al guardar usuario: " + ex.Message;
                    }
                }
            }
            return respuesta;
        }

        public async Task<List<Usuario>> Lista(int IdRolUsuario = 0)
        {
            List<Usuario> lista = new List<Usuario>();

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listaUsuario(@p_IdRolUsuario)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_IdRolUsuario", IdRolUsuario);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Usuario()
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                                Nombre = dr["Nombre"].ToString()!,
                                Apellido = dr["Apellido"].ToString()!,
                                Correo = dr["Correo"].ToString()!,
                                Clave = dr["Clave"].ToString()!,
                                RolUsuario = new RolUsuario()
                                {
                                    IdRolUsuario = Convert.ToInt32(dr["IdRolUsuario"]),
                                    Nombre = dr["NombreRol"].ToString()!,
                                },
                                FechaCreacion = dr["FechaCreacion"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Usuario> Login(string DocumentoIdentidad, string Clave)
        {
            Usuario objeto = null!;

            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_loginUsuario(@p_Doc, @p_Cla)", conexion))
                {
                    cmd.Parameters.AddWithValue("p_Doc", DocumentoIdentidad);
                    cmd.Parameters.AddWithValue("p_Cla", Clave);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            objeto = new Usuario()
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                                Nombre = dr["Nombre"].ToString()!,
                                Apellido = dr["Apellido"].ToString()!,
                                Correo = dr["Correo"].ToString()!,
                                RolUsuario = new RolUsuario
                                {
                                    Nombre = dr["NombreRol"].ToString()!,
                                }
                            };
                        }
                    }
                }
            }
            return objeto;
        }
    }
}