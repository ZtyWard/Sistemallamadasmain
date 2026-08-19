using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using SimuladorLlamadas.Configuracion;

namespace SimuladorLlamadas.Servicios
{
    public static class ServicioConexion
    {
        public static async Task<bool> probar_identificador_async()
        {
            try
            {
                using TcpClient cliente = new TcpClient();
                Task tarea_conexion = cliente.ConnectAsync(
                    ConfiguracionSistema.ip_identificador,
                    ConfiguracionSistema.puerto_identificador);

                Task tarea_timeout = Task.Delay(1500);
                Task completada = await Task.WhenAny(tarea_conexion, tarea_timeout);

                return completada == tarea_conexion && cliente.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}
