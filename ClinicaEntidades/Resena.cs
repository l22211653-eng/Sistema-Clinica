using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class Resena
    {
        public int IdResena { get; set; }
        public int IdCita { get; set; }
        public int IdDoctor { get; set; }
        public int IdUsuario { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; } = null!;
        public string Paciente { get; set; } = null!; // Para mostrar en la vista
        public string Fecha { get; set; } = null!;
    }
}
