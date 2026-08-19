using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuladorLlamadas.Modelos
{
    public class TelefonoPrueba
    {
        public string nombre { get; set; } = string.Empty;

        public string cliente { get; set; } = string.Empty;

        public string proveedor { get; set; } = string.Empty;

        public string estado { get; set; } = "Activo";

        public string tipo_servicio { get; set; } = "PREPAGO";

        public string numero_telefono { get; set; } = string.Empty;

        public string identificador_telefono { get; set; } = string.Empty;

        public string identificador_chip { get; set; } = string.Empty;

        public string coordenadas { get; set; } = string.Empty;

        public string telefono_destino { get; set; } = string.Empty;

        public string tiempo_maximo { get; set; } = string.Empty;

        public override string ToString()
        {
            return nombre + " - " + numero_telefono;
        }
    }
}
