using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class Receta
    {
        public int IdReceta { get; set; }
        public int IdCita { get; set; }
        public string Medicamento { get; set; } = null!;
        public string Dosis { get; set; } = null!;
        public string Indicaciones { get; set; } = null!;

    }
}
