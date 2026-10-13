using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class RendimientoDoctor
    {
        public string Especialidad { get; set; }

        public string Doctor { get; set; }

        public int TotalAtendidas { get; set; }
    }
}