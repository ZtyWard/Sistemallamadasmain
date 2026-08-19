using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SimuladorLlamadas.Servicios
{
    public static class ServicioValidaciones
    {
        public static bool es_numero_telefono_valido(string numero_telefono)
        {
            return Regex.IsMatch(numero_telefono.Trim(), @"^\d{8}$");
        }

        public static bool es_identificador_telefono_valido(string identificador_telefono)
        {
            return Regex.IsMatch(identificador_telefono.Trim(), @"^\d{16}$");
        }

        public static bool es_identificador_chip_valido(string identificador_chip)
        {
            return Regex.IsMatch(identificador_chip.Trim(), @"^\d{19}$");
        }

        public static bool es_codigo_saldo(string codigo)
        {
            return codigo.Trim() == "#9090*";
        }

        public static bool es_numero_marcado_valido(string numero_marcado)
        {
            string valor;

            valor = numero_marcado.Trim();

            if (es_codigo_saldo(valor))
            {
                return true;
            }

            if (es_numero_telefono_valido(valor))
            {
                return true;
            }

            if (Regex.IsMatch(valor, @"^\d{9}$") && valor.EndsWith("1"))
            {
                return true;
            }

            return Regex.IsMatch(valor, @"^00\d{9,18}$");
        }

        public static bool es_tiempo_maximo_valido(string tiempo_maximo)
        {
            if (string.IsNullOrWhiteSpace(tiempo_maximo))
            {
                return false;
            }

            return Regex.IsMatch(tiempo_maximo.Trim(), @"^\d{6}$");
        }

        public static bool es_coordenada_valida(string coordenadas)
        {
            string[] partes;
            double latitud;
            double longitud;

            partes = coordenadas.Trim().Split(',');

            if (partes.Length != 2)
            {
                return false;
            }

            if (!double.TryParse(partes[0], NumberStyles.Any, CultureInfo.InvariantCulture, out latitud))
            {
                return false;
            }

            if (!double.TryParse(partes[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longitud))
            {
                return false;
            }

            if (latitud < -90 || latitud > 90)
            {
                return false;
            }

            if (longitud < -180 || longitud > 180)
            {
                return false;
            }

            return true;
        }

        public static bool es_llave_aes_base64_valida(string llave_aes_base64)
        {
            byte[] datos_llave;

            if (string.IsNullOrWhiteSpace(llave_aes_base64))
            {
                return false;
            }

            try
            {
                datos_llave = Convert.FromBase64String(llave_aes_base64.Trim());

                return datos_llave.Length == 16 ||
                       datos_llave.Length == 24 ||
                       datos_llave.Length == 32;
            }
            catch
            {
                return false;
            }
        }

        public static bool es_puerto_valido(int puerto)
        {
            return puerto >= 1 && puerto <= 65535;
        }
    }
}
