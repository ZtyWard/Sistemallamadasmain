using SimuladorLlamadas.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuladorLlamadas.Configuracion
{
    public static class DatosPrueba
    {
        public const string numero_telefono = "25743715";

        public const string identificador_telefono = "1234567890123456";

        public const string identificador_chip = "1234567890123456789";

        public const string coordenadas = "9.8644,-83.9194";

        public const string telefono_destino = "89154242";

        public const string tiempo_maximo = "001025";

        public const string codigo_consulta_saldo = "#9090*";

        public static List<TelefonoPrueba> obtener_telefonos_prueba()
        {
            List<TelefonoPrueba> telefonos_prueba;

            telefonos_prueba = new List<TelefonoPrueba>();

            telefonos_prueba.Add(new TelefonoPrueba
            {
                nombre = "Teléfono 1",
                cliente = "Juan Perez",
                proveedor = "Proveedor principal",
                estado = "Activo",
                tipo_servicio = "PREPAGO",
                numero_telefono = "25743715",
                identificador_telefono = "1234567890123456",
                identificador_chip = "1234567890123456789",
                coordenadas = "9.8644,-83.9194",
                telefono_destino = "89154242",
                tiempo_maximo = "001025"
            });

            telefonos_prueba.Add(new TelefonoPrueba
            {
                nombre = "Teléfono 2",
                cliente = "Maria Rodriguez",
                proveedor = "Proveedor principal",
                estado = "Activo",
                tipo_servicio = "PREPAGO",
                numero_telefono = "25262020",
                identificador_telefono = "2222222222222222",
                identificador_chip = "2222222222222222222",
                coordenadas = "9.9281,-84.0907",
                telefono_destino = "88889999",
                tiempo_maximo = "000530"
            });

            telefonos_prueba.Add(new TelefonoPrueba
            {
                nombre = "Teléfono 3",
                cliente = "Carlos Solano",
                proveedor = "Proveedor principal",
                estado = "Activo",
                tipo_servicio = "PREPAGO",
                numero_telefono = "22334455",
                identificador_telefono = "3333333333333333",
                identificador_chip = "3333333333333333333",
                coordenadas = "9.9333,-84.0833",
                telefono_destino = "70001122",
                tiempo_maximo = "001500"
            });

            telefonos_prueba.Add(new TelefonoPrueba
            {
                nombre = "Teléfono 4 (Postpago)",
                cliente = "Ana Vargas",
                proveedor = "Proveedor secundario",
                estado = "Activo",
                tipo_servicio = "POSTPAGO",
                numero_telefono = "89154242",
                identificador_telefono = "4444444444444444",
                identificador_chip = "4444444444444444444",
                coordenadas = "9.9350,-84.0910",
                telefono_destino = "25743715",
                tiempo_maximo = "245959"
            });

            return telefonos_prueba;
        }
    }
}
