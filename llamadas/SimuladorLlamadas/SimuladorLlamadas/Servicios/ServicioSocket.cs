using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimuladorLlamadas.Configuracion;

namespace SimuladorLlamadas.Servicios
{
    public static class ServicioSocket
    {
        public static async Task<string> enviar_mensaje_async(string mensaje_json)
        {
            TcpClient cliente_socket;
            NetworkStream flujo_red;
            byte[] datos_envio;
            byte[] datos_respuesta;
            int cantidad_bytes_recibidos;
            string mensaje_con_salto;
            string respuesta_servidor;

            try
            {
                cliente_socket = new TcpClient();

                await cliente_socket.ConnectAsync(
                    ConfiguracionSistema.ip_identificador,
                    ConfiguracionSistema.puerto_identificador
                );

                flujo_red = cliente_socket.GetStream();

                mensaje_con_salto = mensaje_json + "\n";
                datos_envio = Encoding.UTF8.GetBytes(mensaje_con_salto);

                await flujo_red.WriteAsync(datos_envio, 0, datos_envio.Length);

                datos_respuesta = new byte[4096];
                cantidad_bytes_recibidos = await flujo_red.ReadAsync(
                    datos_respuesta,
                    0,
                    datos_respuesta.Length
                );

                respuesta_servidor = Encoding.UTF8.GetString(
                    datos_respuesta,
                    0,
                    cantidad_bytes_recibidos
                );

                cliente_socket.Close();

                return respuesta_servidor.Trim();
            }
            catch (SocketException)
            {
                return "No fue posible conectar con el Identificador.\r\n" +
                       "Verifique que el servidor esté activo y que la IP y el puerto sean correctos.";
            }
            catch (Exception error)
            {
                return "Ocurrió un error al enviar la solicitud.\r\n" +
                       "Detalle: " + error.Message;
            }
        }
    }
}