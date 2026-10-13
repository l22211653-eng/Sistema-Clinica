using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaData.Configuracion
{
    public class ConnectionStrings
    {
        // Ahora esta propiedad guarda la cadena de conexión a PostgreSQL (Supabase)
        public string CadenaSQL { get; set; } = null!;
    }
}