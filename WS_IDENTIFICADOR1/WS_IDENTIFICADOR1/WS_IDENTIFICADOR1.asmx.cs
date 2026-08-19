using System;
using System.Net.Sockets;
using System.Text;
using System.Web.Services;

namespace WS_IDENTIFICADOR1
{
    // ============================================================
    // WS_IDENTIFICADOR1
    // Web Service SOAP para consultar saldo.
    //
    // Recibe:
    // - telefonoEncriptado
    // - origen = WEB
    // - tipoTransaccion = saldo
    //
    // Envía al Identificador Python:
    // WS_SALDO|telefonoEnc|WEB|saldo
    //
    // Responde:
    // Resultado = true / false
    // Mensaje = Exitoso / error
    // Saldo = saldo recibido
    // ============================================================

    [WebService(Namespace = "http://centralgeneral/identificador")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class WSIdentificador : WebService
    {
        private const string HOST_IDENTIFICADOR = "127.0.0.1";
        private const int PUERTO_IDENTIFICADOR = 5000;

        [WebMethod]
        public RespuestaSaldo ConsultarSaldo(string telefonoEncriptado, string origen, string tipoTransaccion)
        {
            try
            {
                // ====================================================
                // Validar datos obligatorios
                // ====================================================
                if (string.IsNullOrWhiteSpace(telefonoEncriptado) ||
                    string.IsNullOrWhiteSpace(origen) ||
                    string.IsNullOrWhiteSpace(tipoTransaccion))
                {
                    return new RespuestaSaldo
                    {
                        Resultado = false,
                        Mensaje = "Datos incorrectos o incompletos.",
                        Saldo = ""
                    };
                }

                // ====================================================
                // El alcance dice que el origen Web debe distinguirse.
                // Para esta operación aceptamos solamente WEB.
                // ====================================================
                if (origen.Trim().ToUpper() != "WEB")
                {
                    return new RespuestaSaldo
                    {
                        Resultado = false,
                        Mensaje = "Origen inválido.",
                        Saldo = ""
                    };
                }

                // ====================================================
                // Esta operación es solamente para saldo.
                // ====================================================
                if (tipoTransaccion.Trim().ToLower() != "saldo")
                {
                    return new RespuestaSaldo
                    {
                        Resultado = false,
                        Mensaje = "Tipo de transacción inválido.",
                        Saldo = ""
                    };
                }

                // ====================================================
                // Armar trama para enviar al socket Python.
                // Formato esperado por el Identificador:
                // WS_SALDO|telefonoEnc|WEB|saldo
                // ====================================================
                string trama = "WS_SALDO|" +
                               telefonoEncriptado.Trim() + "|" +
                               origen.Trim().ToUpper() + "|" +
                               tipoTransaccion.Trim().ToLower();

                string respuestaIdentificador = EnviarAlIdentificador(trama);

                // ====================================================
                // Respuesta esperada desde Python:
                // OK|0000000000000125000
                // ====================================================
                if (respuestaIdentificador.StartsWith("OK|"))
                {
                    string[] partes = respuestaIdentificador.Split('|');

                    return new RespuestaSaldo
                    {
                        Resultado = true,
                        Mensaje = "Exitoso",
                        Saldo = partes[1]
                    };
                }

                return new RespuestaSaldo
                {
                    Resultado = false,
                    Mensaje = "Problemas al consultar saldo.",
                    Saldo = ""
                };
            }
            catch (Exception ex)
            {
                return new RespuestaSaldo
                {
                    Resultado = false,
                    Mensaje = "ERROR: " + ex.Message,
                    Saldo = ""
                };
            }
        }

        // ============================================================
        // Envía la trama al socket Python del Identificador.
        // El Identificador debe estar corriendo en el puerto 5000.
        // ============================================================
        private string EnviarAlIdentificador(string trama)
        {
            using (TcpClient cliente = new TcpClient())
            {
                cliente.Connect(HOST_IDENTIFICADOR, PUERTO_IDENTIFICADOR);

                NetworkStream stream = cliente.GetStream();

                byte[] datosEnviar = Encoding.UTF8.GetBytes(trama + "\n");
                stream.Write(datosEnviar, 0, datosEnviar.Length);

                byte[] buffer = new byte[4096];
                int bytesLeidos = stream.Read(buffer, 0, buffer.Length);

                return Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();
            }
        }
    }

    // ============================================================
    // Esta clase define la estructura que devuelve el SOAP.
    // En el navegador se verá como XML.
    // ============================================================
    public class RespuestaSaldo
    {
        public bool Resultado { get; set; }
        public string Mensaje { get; set; }
        public string Saldo { get; set; }
    }
}