using System;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace SimuladorLlamadas.Servicios
{
    public static class ServicioFormato
    {
        public static string clasificar_destino(string numero_marcado)
        {
            string valor = numero_marcado.Trim();

            if (ServicioValidaciones.es_codigo_saldo(valor))
            {
                return "Consulta de saldo";
            }

            if (valor.StartsWith("00") || valor.StartsWith("+"))
            {
                return "Llamada internacional";
            }

            return "Llamada nacional";
        }

        public static bool es_llamada_internacional(string numero_marcado)
        {
            string valor = numero_marcado.Trim();
            return valor.StartsWith("00") || valor.StartsWith("+");
        }

        public static int hms_a_segundos(string hms)
        {
            string valor = (hms ?? string.Empty).Trim().PadLeft(6, '0');

            if (valor.Length > 6)
            {
                valor = valor.Substring(valor.Length - 6);
            }

            if (!int.TryParse(valor.Substring(0, 2), out int horas))
            {
                horas = 0;
            }

            if (!int.TryParse(valor.Substring(2, 2), out int minutos))
            {
                minutos = 0;
            }

            if (!int.TryParse(valor.Substring(4, 2), out int segundos))
            {
                segundos = 0;
            }

            return horas * 3600 + minutos * 60 + segundos;
        }

        public static string segundos_a_reloj(int segundos)
        {
            TimeSpan tiempo = TimeSpan.FromSeconds(Math.Max(0, segundos));
            return tiempo.ToString(@"hh\:mm\:ss");
        }

        public static string saldo_proveedor_a_colones(string valor)
        {
            string saldo = (valor ?? string.Empty).Trim();

            if (saldo == "-1")
            {
                return "-1 (postpago)";
            }

            if (!long.TryParse(saldo, out long centavos))
            {
                return saldo;
            }

            decimal monto = centavos / 100m;
            return monto.ToString("C2", new CultureInfo("es-CR"));
        }

        public static string monto_proveedor_a_colones(string valor)
        {
            return saldo_proveedor_a_colones(valor);
        }

        public static string respuesta_amigable(string respuesta_servidor)
        {
            if (string.IsNullOrWhiteSpace(respuesta_servidor))
            {
                return "Sin respuesta del Identificador.";
            }

            if (!respuesta_servidor.TrimStart().StartsWith("{"))
            {
                return respuesta_servidor;
            }

            try
            {
                using JsonDocument documento = JsonDocument.Parse(respuesta_servidor);
                JsonElement raiz = documento.RootElement;
                StringBuilder texto = new StringBuilder();

                string status = obtener_string(raiz, "status");
                if (string.IsNullOrWhiteSpace(status))
                {
                    status = obtener_string(raiz, "estado");
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    texto.AppendLine("Estado: " + traducir_estado(status));
                }

                agregar_si_existe(texto, raiz, "tiempo", "Tiempo autorizado", true);
                agregar_si_existe(texto, raiz, "monto_autorizado", "Monto autorizado", false);
                agregar_si_existe(texto, raiz, "tarifa", "Tarifa por minuto", false);
                agregar_si_existe(texto, raiz, "llamada_id", "Llamada ID", false);
                agregar_si_existe(texto, raiz, "duracion_real", "Duracion real", true);
                agregar_si_existe(texto, raiz, "costo", "Costo", false);
                agregar_si_existe(texto, raiz, "saldo", "Saldo", false);
                agregar_si_existe(texto, raiz, "razon", "Razon", false);
                agregar_si_existe(texto, raiz, "detalle", "Detalle", false);

                if (raiz.TryGetProperty("motivo", out JsonElement motivo))
                {
                    int codigo = motivo.ValueKind == JsonValueKind.Number ? motivo.GetInt32() : 0;
                    texto.AppendLine("Codigo: " + codigo);
                    texto.AppendLine("Motivo: " + traducir_codigo_error(codigo));
                }

                if (texto.Length == 0)
                {
                    return respuesta_servidor;
                }

                return texto.ToString().Trim();
            }
            catch
            {
                return respuesta_servidor;
            }
        }

        public static string traducir_codigo_error(int codigo)
        {
            return codigo switch
            {
                1 => "Destino u origen invalido.",
                2 => "El telefono y la tarjeta SIM no coinciden.",
                3 => "El telefono esta fuera de Costa Rica.",
                4 => "Accion invalida o saldo insuficiente.",
                5 => "Error de proveedor, codigo de pais invalido o comunicacion.",
                _ => "Error no identificado."
            };
        }

        private static void agregar_si_existe(
            StringBuilder texto,
            JsonElement raiz,
            string propiedad,
            string etiqueta,
            bool es_tiempo)
        {
            string valor = obtener_string(raiz, propiedad);

            if (string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            if (propiedad == "saldo" || propiedad == "monto_autorizado" || propiedad == "tarifa" || propiedad == "costo")
            {
                valor = monto_proveedor_a_colones(valor);
            }
            else if (es_tiempo)
            {
                valor = valor + " (" + segundos_a_reloj(hms_a_segundos(valor)) + ")";
            }

            texto.AppendLine(etiqueta + ": " + valor);
        }

        private static string obtener_string(JsonElement raiz, string propiedad)
        {
            if (!raiz.TryGetProperty(propiedad, out JsonElement elemento))
            {
                return string.Empty;
            }

            return elemento.ValueKind switch
            {
                JsonValueKind.String => elemento.GetString() ?? string.Empty,
                JsonValueKind.Number => elemento.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => elemento.ToString()
            };
        }

        private static string traducir_estado(string estado)
        {
            return estado.ToUpperInvariant() switch
            {
                "OK" => "OK",
                "FALLIDO" => "Error",
                "FINALIZADA" => "Finalizada",
                "ERROR" => "Error",
                _ => estado
            };
        }
    }
}
