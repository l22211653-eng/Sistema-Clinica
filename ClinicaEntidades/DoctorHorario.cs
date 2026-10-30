using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class DoctorHorario
    {
        public int IdDoctorHorario { get; set; }
        public int IdDoctor { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int NumeroMes { get; set; }
        public string? HoraInicioAM { get; set; }
        public string? HoraFinAM { get; set; }
        public string? HoraInicioPM { get; set; }
        public string? HoraFinPM { get; set; }
        public string? FechaCreacion { get; set; }
        public DoctorHorarioDetalle DoctorHorarioDetalle { get; set; } = null!;

    }
}
