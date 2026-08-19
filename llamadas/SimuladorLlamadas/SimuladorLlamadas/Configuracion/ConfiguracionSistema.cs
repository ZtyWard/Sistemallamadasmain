using System;
using System.IO;
using System.Text.Json;

namespace SimuladorLlamadas.Configuracion
{
    public static class ConfiguracionSistema
    {
        private static readonly string ruta_archivo_configuracion =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuracion_simulador.json");

        public static string ip_identificador { get; set; } = "127.0.0.1";

        public static int puerto_identificador { get; set; } = 5000;

        public static string llave_aes_base64 { get; set; } = "";

        public static void cargar_configuracion()
        {
            ConfiguracionArchivo? configuracion_archivo;
            string contenido_json;

            try
            {
                if (!File.Exists(ruta_archivo_configuracion))
                {
                    return;
                }

                contenido_json = File.ReadAllText(ruta_archivo_configuracion);

                configuracion_archivo = JsonSerializer.Deserialize<ConfiguracionArchivo>(contenido_json);

                if (configuracion_archivo == null)
                {
                    return;
                }

                ip_identificador = configuracion_archivo.ip_identificador;
                puerto_identificador = configuracion_archivo.puerto_identificador;
                llave_aes_base64 = configuracion_archivo.llave_aes_base64;
            }
            catch
            {
                ip_identificador = "127.0.0.1";
                puerto_identificador = 5000;
                llave_aes_base64 = "";
            }
        }

        public static void guardar_configuracion()
        {
            ConfiguracionArchivo configuracion_archivo;
            JsonSerializerOptions opciones_json;
            string contenido_json;

            configuracion_archivo = new ConfiguracionArchivo();
            configuracion_archivo.ip_identificador = ip_identificador;
            configuracion_archivo.puerto_identificador = puerto_identificador;
            configuracion_archivo.llave_aes_base64 = llave_aes_base64;

            opciones_json = new JsonSerializerOptions();
            opciones_json.WriteIndented = true;

            contenido_json = JsonSerializer.Serialize(configuracion_archivo, opciones_json);

            File.WriteAllText(ruta_archivo_configuracion, contenido_json);
        }

        private class ConfiguracionArchivo
        {
            public string ip_identificador { get; set; } = "127.0.0.1";

            public int puerto_identificador { get; set; } = 5000;

            public string llave_aes_base64 { get; set; } = "";
        }
    }
}