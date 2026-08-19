using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuladorLlamadas.Modelos
{
    public class SolicitudConsultaSaldo
    {
        public string transaccion { get; set; } = string.Empty;

        public string telefono { get; set; } = string.Empty;

        public string identificador_tel { get; set; } = string.Empty;

        public string identificador_chip { get; set; } = string.Empty;

        public string coordenadas { get; set; } = string.Empty;
    }
}