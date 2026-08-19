using System;
using SimuladorLlamadas.Servicios;

namespace SimuladorLlamadas
{
    public static class EstadoLlamadaActual
    {
        public static string telefono_origen { get; private set; } = string.Empty;
        public static string telefono_destino { get; private set; } = string.Empty;
        public static string tiempo_maximo { get; private set; } = string.Empty;
        public static string monto_autorizado { get; private set; } = string.Empty;
        public static string tarifa { get; private set; } = string.Empty;
        public static string ultima_respuesta_identificador { get; private set; } = string.Empty;
        public static string tipo_destino { get; private set; } = string.Empty;
        public static string estado { get; private set; } = "Sin llamada";
        public static int duracion_real_segundos { get; private set; } = 0;

        public static DateTime? inicio_llamada { get; private set; } = null;
        public static bool existe_llamada_pendiente { get; private set; } = false;
        public static bool llamada_activa { get; private set; } = false;

        public static int tiempo_maximo_segundos
        {
            get
            {
                return ServicioFormato.hms_a_segundos(tiempo_maximo);
            }
        }

        public static int tiempo_transcurrido_segundos
        {
            get
            {
                if (!llamada_activa || inicio_llamada == null)
                {
                    return duracion_real_segundos;
                }

                return Math.Max(0, (int)(DateTime.Now - inicio_llamada.Value).TotalSeconds);
            }
        }

        public static int tiempo_restante_segundos
        {
            get
            {
                return Math.Max(0, tiempo_maximo_segundos - tiempo_transcurrido_segundos);
            }
        }

        public static void registrar_llamada_pendiente(
            string telefono_origen_recibido,
            string telefono_destino_recibido,
            string tiempo_maximo_recibido,
            string monto_autorizado_recibido = "",
            string tarifa_recibida = "",
            string ultima_respuesta = "",
            string tipo_destino_recibido = "")
        {
            telefono_origen = telefono_origen_recibido.Trim();
            telefono_destino = telefono_destino_recibido.Trim();
            tiempo_maximo = tiempo_maximo_recibido.Trim();
            monto_autorizado = monto_autorizado_recibido.Trim();
            tarifa = tarifa_recibida.Trim();
            ultima_respuesta_identificador = ultima_respuesta.Trim();
            tipo_destino = string.IsNullOrWhiteSpace(tipo_destino_recibido)
                ? ServicioFormato.clasificar_destino(telefono_destino)
                : tipo_destino_recibido.Trim();

            duracion_real_segundos = 0;
            inicio_llamada = null;
            estado = "Pendiente";
            existe_llamada_pendiente = true;
            llamada_activa = false;
        }

        public static void marcar_llamada_como_activa(string ultima_respuesta = "")
        {
            if (!string.IsNullOrWhiteSpace(ultima_respuesta))
            {
                ultima_respuesta_identificador = ultima_respuesta.Trim();
            }

            inicio_llamada = DateTime.Now;
            duracion_real_segundos = 0;
            estado = "Activa";
            existe_llamada_pendiente = true;
            llamada_activa = true;
        }

        public static void registrar_e_iniciar_llamada_directa(
            string telefono_origen_recibido,
            string telefono_destino_recibido,
            string tiempo_maximo_recibido,
            string ultima_respuesta = "",
            string tipo_destino_recibido = "")
        {
            registrar_llamada_pendiente(
                telefono_origen_recibido,
                telefono_destino_recibido,
                tiempo_maximo_recibido,
                "",
                "",
                ultima_respuesta,
                tipo_destino_recibido
            );
            marcar_llamada_como_activa(ultima_respuesta);
        }

        public static void finalizar_llamada(string ultima_respuesta = "", string estado_final = "Finalizada")
        {
            duracion_real_segundos = tiempo_transcurrido_segundos;

            if (!string.IsNullOrWhiteSpace(ultima_respuesta))
            {
                ultima_respuesta_identificador = ultima_respuesta.Trim();
            }

            estado = estado_final;
            llamada_activa = false;
            existe_llamada_pendiente = false;
            inicio_llamada = null;
        }

        public static void limpiar_llamada_actual()
        {
            telefono_origen = string.Empty;
            telefono_destino = string.Empty;
            tiempo_maximo = string.Empty;
            monto_autorizado = string.Empty;
            tarifa = string.Empty;
            ultima_respuesta_identificador = string.Empty;
            tipo_destino = string.Empty;
            estado = "Sin llamada";
            duracion_real_segundos = 0;
            inicio_llamada = null;
            existe_llamada_pendiente = false;
            llamada_activa = false;
        }

        public static string resumen_llamada()
        {
            if (!existe_llamada_pendiente && !llamada_activa)
            {
                return "No hay llamada activa.";
            }

            return telefono_origen + " -> " + telefono_destino +
                   " | " + estado +
                   " | " + ServicioFormato.segundos_a_reloj(tiempo_transcurrido_segundos) +
                   " / " + ServicioFormato.segundos_a_reloj(tiempo_maximo_segundos);
        }
    }
}
