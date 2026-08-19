using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services;

namespace WS_PROVEEDOR
{
    [WebService(Namespace = "http://centralgeneral/proveedor")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class WSProveedor : WebService
    {
        private const string HOST_PREDETERMINADO = "127.0.0.1";
        private const int PUERTO_PREDETERMINADO = 6000;
        private const int TIMEOUT_PREDETERMINADO_MS = 5000;
        private const string AES_KEY_PREDETERMINADA = "12345678901234567890123456789012";
        private const string SQL_CONNECTION_PREDETERMINADA =
            @"Server=localhost\SQLEXPRESS;Database=ProveedorTelefonicoDB;User ID=javauser;Password=Java123456;Encrypt=False;TrustServerCertificate=True";

        private const string MSG_OK = "Exitoso";
        private const string MSG_INCLUIR_ERROR = "Problemas al incluir la información.";
        private const string MSG_ACTIVAR_ERROR = "Problemas al activar/desactivar la línea.";
        private const string MSG_FACTURAR_ERROR = "Problemas al realizar el cálculo.";

        [WebMethod]
        public RespuestaProveedor IngresarServicioNuevoDisponible(
            string telefonoEncriptado,
            string identificadorTelefonoEncriptado,
            string identificadorTarjetaEncriptado,
            string tipoEncriptado,
            string estadoEncriptado)
        {
            try
            {
                string telefono = DescifrarAES(telefonoEncriptado);
                string identificadorTelefono = DescifrarAES(identificadorTelefonoEncriptado);
                string identificadorTarjeta = DescifrarAES(identificadorTarjetaEncriptado);
                string tipo = DescifrarAES(tipoEncriptado).Trim().ToUpper();
                string estado = DescifrarAES(estadoEncriptado).Trim().ToUpper();

                if (!TelefonoValido(telefono)
                    || !IdentificadorTelefonoValido(identificadorTelefono)
                    || !IdentificadorTarjetaValido(identificadorTarjeta)
                    || !TipoServicioValido(tipo)
                    || estado != "DISPONIBLE")
                {
                    return Error(MSG_INCLUIR_ERROR);
                }

                string trama = string.Join("|",
                    "PROVEEDOR4",
                    telefonoEncriptado.Trim(),
                    identificadorTelefonoEncriptado.Trim(),
                    identificadorTarjetaEncriptado.Trim(),
                    tipo,
                    estado);

                string respuesta = EnviarProveedor(trama);
                return EsRespuestaExitosa(respuesta) ? Ok() : Error(MSG_INCLUIR_ERROR);
            }
            catch
            {
                return Error(MSG_INCLUIR_ERROR);
            }
        }

        [WebMethod]
        public RespuestaProveedor ActivarDesactivarServicio(
            string telefonoEncriptado,
            string identificadorTelefonoEncriptado,
            string identificadorTarjetaEncriptado,
            string tipoEncriptado,
            string identificacionClienteEncriptada,
            string estadoEncriptado)
        {
            try
            {
                string telefono = DescifrarAES(telefonoEncriptado);
                string identificadorTelefono = DescifrarAES(identificadorTelefonoEncriptado);
                string identificadorTarjeta = DescifrarAES(identificadorTarjetaEncriptado);
                string tipo = DescifrarAES(tipoEncriptado).Trim().ToUpper();
                string identificacionCliente = DescifrarAES(identificacionClienteEncriptada);
                string estado = DescifrarAES(estadoEncriptado).Trim().ToUpper();

                if (!TelefonoValido(telefono)
                    || !IdentificadorTelefonoValido(identificadorTelefono)
                    || !IdentificadorTarjetaValido(identificadorTarjeta)
                    || !TipoServicioValido(tipo)
                    || !IdentificacionValida(identificacionCliente)
                    || !EstadoActivacionValido(estado))
                {
                    return Error(MSG_ACTIVAR_ERROR);
                }

                string trama = string.Join("|",
                    "PROVEEDOR5",
                    telefonoEncriptado.Trim(),
                    identificadorTelefonoEncriptado.Trim(),
                    identificadorTarjetaEncriptado.Trim(),
                    tipo,
                    identificacionCliente.Trim(),
                    estado);

                string respuesta = EnviarProveedor(trama);
                return EsRespuestaExitosa(respuesta) ? Ok() : Error(MSG_ACTIVAR_ERROR);
            }
            catch
            {
                return Error(MSG_ACTIVAR_ERROR);
            }
        }

        [WebMethod]
        public RespuestaProveedor CalcularFacturacion(string fechaCalculo, string fechaMaximaPago)
        {
            try
            {
                if (!FechaValida(fechaCalculo) || !FechaValida(fechaMaximaPago))
                {
                    return Error(MSG_FACTURAR_ERROR);
                }

                DateTime calculo = DateTime.ParseExact(fechaCalculo.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime maxima = DateTime.ParseExact(fechaMaximaPago.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (maxima < calculo)
                {
                    return Error(MSG_FACTURAR_ERROR);
                }

                string trama = string.Join("|",
                    "PROVEEDOR6",
                    calculo.ToString("yyyy-MM-dd"),
                    maxima.ToString("yyyy-MM-dd"));

                string respuesta = EnviarProveedor(trama);
                return EsRespuestaExitosa(respuesta) ? Ok() : Error(MSG_FACTURAR_ERROR);
            }
            catch
            {
                return Error(MSG_FACTURAR_ERROR);
            }
        }

        [WebMethod]
        public string EncriptarTexto(string texto)
        {
            return CifrarAES(texto);
        }

        [WebMethod]
        public RespuestaProveedor ProbarConexionProveedor()
        {
            try
            {
                string respuesta = EnviarProveedor("PING");
                return EsRespuestaExitosa(respuesta) ? Ok() : Error("Proveedor no respondió correctamente.");
            }
            catch (Exception ex)
            {
                return Error("Error conectando con proveedor: " + ex.Message);
            }
        }

        [WebMethod]
        public string ObtenerUltimaFechaFacturacion()
        {
            try
            {
                return EnviarProveedor("PROVEEDOR6|ULTIMA_FECHA");
            }
            catch
            {
                return "ERROR";
            }
        }

        [WebMethod]
        public RespuestaLineasProveedor ListarLineasDisponibles()
        {
            return ListarLineasAdministrativas("DISPONIBLE", false);
        }

        [WebMethod]
        public RespuestaLineasProveedor ListarLineasActivas()
        {
            return ListarLineasAdministrativas("ACTIVO", true);
        }

        [WebMethod]
        public RespuestaProveedor EliminarLineaDisponible(string telefono)
        {
            if (!TelefonoValido(telefono))
            {
                return Error("Datos incorrectos o incompletos.");
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(CadenaConexionProveedor))
                {
                    conexion.Open();

                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                        const string verificar = @"
SELECT COUNT(*)
FROM dbo.telefonos WITH (UPDLOCK, HOLDLOCK)
WHERE telefono = @telefono
  AND estado = 'DISPONIBLE'
  AND cliente_id IS NULL;";

                        using (SqlCommand comando = new SqlCommand(verificar, conexion, transaccion))
                        {
                            comando.Parameters.Add("@telefono", SqlDbType.VarChar, 20).Value = telefono.Trim();
                            int cantidad = Convert.ToInt32(
                                comando.ExecuteScalar(), CultureInfo.InvariantCulture);

                            if (cantidad != 1)
                            {
                                transaccion.Rollback();
                                return Error("La línea no está disponible para eliminar.");
                            }
                        }

                        const string eliminarTarjeta = @"
DELETE ta
FROM dbo.tarjetas ta
INNER JOIN dbo.telefonos t ON t.id = ta.telefono_id
WHERE t.telefono = @telefono;";

                        using (SqlCommand comando = new SqlCommand(
                            eliminarTarjeta, conexion, transaccion))
                        {
                            comando.Parameters.Add("@telefono", SqlDbType.VarChar, 20).Value = telefono.Trim();
                            comando.ExecuteNonQuery();
                        }

                        const string eliminarTelefono = @"
DELETE FROM dbo.telefonos
WHERE telefono = @telefono
  AND estado = 'DISPONIBLE'
  AND cliente_id IS NULL;";

                        using (SqlCommand comando = new SqlCommand(
                            eliminarTelefono, conexion, transaccion))
                        {
                            comando.Parameters.Add("@telefono", SqlDbType.VarChar, 20).Value = telefono.Trim();

                            if (comando.ExecuteNonQuery() != 1)
                            {
                                transaccion.Rollback();
                                return Error("No fue posible eliminar la línea.");
                            }
                        }

                        // Los DELETE de SQL Server todavia no se han confirmado.
                        // Si Python rechaza la eliminacion (por ejemplo, porque
                        // existen llamadas), el rollback restaura telefono y tarjeta.
                        string respuestaIdentificador = EnviarProveedor(
                            "PROVEEDOR4_ELIMINAR|" + telefono.Trim());

                        if (!EsRespuestaExitosa(respuestaIdentificador))
                        {
                            transaccion.Rollback();
                            return Error("No fue posible eliminar la línea del Identificador.");
                        }

                            transaccion.Commit();
                            return Ok();
                        }
                        catch
                        {
                            try
                            {
                                transaccion.Rollback();
                            }
                            catch
                            {
                            }

                            return Error("No fue posible eliminar la línea.");
                        }
                    }
                }
            }
            catch
            {
                return Error("No fue posible eliminar la línea.");
            }
        }

        [WebMethod]
        public string ListarLineasCliente(string identificacionCliente)
        {
            return EnviarConsultaCliente("CLIENTE4|LISTAR|", identificacionCliente);
        }

        [WebMethod]
        public string ListarLineasPrepagoCliente(string identificacionCliente)
        {
            return EnviarConsultaCliente("CLIENTE5|LISTAR|", identificacionCliente);
        }

        [WebMethod]
        public string ListarFacturasPendientesCliente(string identificacionCliente)
        {
            return EnviarConsultaCliente("CLIENTE6|LISTAR|", identificacionCliente);
        }

        [WebMethod]
        public string ListarLineasDevolucionCliente(string identificacionCliente)
        {
            return EnviarConsultaCliente("CLIENTE7|LISTAR|", identificacionCliente);
        }

        [WebMethod]
        public RespuestaProveedor RecargarSaldo(string telefono, decimal monto)
        {
            if (!TelefonoValido(telefono) || monto <= 0 || monto != Math.Truncate(monto))
            {
                return Error("Datos incorrectos o incompletos.");
            }

            try
            {
                string trama = "RECARGA|" + telefono.Trim() + "|"
                    + monto.ToString("0", CultureInfo.InvariantCulture);
                return EsRespuestaExitosa(EnviarProveedor(trama))
                    ? Ok()
                    : Error("No fue posible cargar el saldo.");
            }
            catch
            {
                return Error("No fue posible cargar el saldo.");
            }
        }

        [WebMethod]
        public RespuestaProveedor PagarFactura(int facturaId)
        {
            if (facturaId <= 0)
            {
                return Error("Factura incorrecta.");
            }

            try
            {
                return EsRespuestaExitosa(
                    EnviarProveedor("PAGAR_FACTURA|" + facturaId))
                    ? Ok()
                    : Error("No fue posible cancelar la factura.");
            }
            catch
            {
                return Error("No fue posible cancelar la factura.");
            }
        }

        [WebMethod]
        public RespuestaProveedor DevolverLinea(string telefono)
        {
            if (!TelefonoValido(telefono))
            {
                return Error("Datos incorrectos o incompletos.");
            }

            try
            {
                string respuesta = EnviarProveedor(
                    "CLIENTE7|DEVOLVER|" + telefono.Trim());

                return EsRespuestaExitosa(respuesta)
                    ? Ok()
                    : Error(respuesta);
            }
            catch
            {
                return Error("No fue posible devolver la línea.");
            }
        }

        private RespuestaLineasProveedor ListarLineasAdministrativas(
            string estado,
            bool requiereCliente)
        {
            var respuesta = new RespuestaLineasProveedor
            {
                Resultado = false,
                Mensaje = "No fue posible consultar las líneas.",
                Lineas = new List<LineaAdministrativa>()
            };

            string condicionCliente = requiereCliente
                ? "t.cliente_id IS NOT NULL"
                : "t.cliente_id IS NULL";

            string estadoTarjeta = requiereCliente
                ? "ACTIVA"
                : "DISPONIBLE";

            string consulta = @"
SELECT
    t.telefono,
    t.identificador_telefono,
    ta.identificador_tarjeta,
    t.tipo_servicio,
    t.estado,
    ISNULL(c.identificacion, '') AS identificacion_cliente
FROM dbo.telefonos t
INNER JOIN dbo.tarjetas ta ON ta.telefono_id = t.id
LEFT JOIN dbo.clientes c ON c.id = t.cliente_id
WHERE t.estado = @estado
  AND ta.estado = @estadoTarjeta
  AND " + condicionCliente + @"
ORDER BY t.telefono;";

            try
            {
                using (SqlConnection conexion = new SqlConnection(CadenaConexionProveedor))
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@estado", SqlDbType.VarChar, 15).Value = estado;
                    comando.Parameters.Add("@estadoTarjeta", SqlDbType.VarChar, 15).Value = estadoTarjeta;
                    conexion.Open();

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            respuesta.Lineas.Add(new LineaAdministrativa
                            {
                                Telefono = Convert.ToString(
                                    lector["telefono"], CultureInfo.InvariantCulture),
                                IdentificadorTelefono = Convert.ToString(
                                    lector["identificador_telefono"], CultureInfo.InvariantCulture),
                                IdentificadorTarjeta = Convert.ToString(
                                    lector["identificador_tarjeta"], CultureInfo.InvariantCulture),
                                TipoServicio = Convert.ToString(
                                    lector["tipo_servicio"], CultureInfo.InvariantCulture),
                                Estado = Convert.ToString(
                                    lector["estado"], CultureInfo.InvariantCulture),
                                IdentificacionCliente = Convert.ToString(
                                    lector["identificacion_cliente"], CultureInfo.InvariantCulture)
                            });
                        }
                    }
                }

                respuesta.Resultado = true;
                respuesta.Mensaje = MSG_OK;
            }
            catch
            {
                respuesta.Lineas.Clear();
            }

            return respuesta;
        }

        private string EnviarConsultaCliente(
            string prefijo,
            string identificacionCliente)
        {
            if (!IdentificacionValida(identificacionCliente))
            {
                return "ERROR";
            }

            try
            {
                return EnviarProveedor(prefijo + identificacionCliente.Trim());
            }
            catch
            {
                return "ERROR";
            }
        }

        private string EnviarProveedor(string trama)
        {
            using (TcpClient cliente = new TcpClient())
            {
                IAsyncResult conexion = cliente.BeginConnect(HostProveedor, PuertoProveedor, null, null);

                if (!conexion.AsyncWaitHandle.WaitOne(TimeoutProveedorMs))
                {
                    throw new TimeoutException("Tiempo de conexión agotado.");
                }

                cliente.EndConnect(conexion);
                cliente.SendTimeout = TimeoutProveedorMs;
                cliente.ReceiveTimeout = TimeoutProveedorMs;

                using (NetworkStream stream = cliente.GetStream())
                {
                    byte[] datos = Encoding.UTF8.GetBytes(trama + "\n");
                    stream.Write(datos, 0, datos.Length);

                    byte[] buffer = new byte[4096];
                    int leidos = stream.Read(buffer, 0, buffer.Length);
                    return Encoding.UTF8.GetString(buffer, 0, leidos).Trim();
                }
            }
        }

        private bool EsRespuestaExitosa(string respuesta)
        {
            return !string.IsNullOrWhiteSpace(respuesta)
                && respuesta.Trim().Equals("OK", StringComparison.OrdinalIgnoreCase);
        }

        private bool TelefonoValido(string valor)
        {
            return Regex.IsMatch(Normalizar(valor), @"^\d{8}$");
        }

        private bool IdentificadorTelefonoValido(string valor)
        {
            return Regex.IsMatch(Normalizar(valor), @"^\d{16}$");
        }

        private bool IdentificadorTarjetaValido(string valor)
        {
            return Regex.IsMatch(Normalizar(valor), @"^\d{19}$");
        }

        private bool IdentificacionValida(string valor)
        {
            return Regex.IsMatch(Normalizar(valor), @"^\d+$");
        }

        private bool TipoServicioValido(string valor)
        {
            string tipo = Normalizar(valor).ToUpper();
            return tipo == "PREPAGO" || tipo == "POSTPAGO";
        }

        private bool EstadoActivacionValido(string valor)
        {
            string estado = Normalizar(valor).ToUpper();
            return estado == "ACTIVO"
                || estado == "INACTIVO"
                || estado == "DISPONIBLE";
        }

        private bool FechaValida(string valor)
        {
            DateTime fecha;
            return DateTime.TryParseExact(Normalizar(valor), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out fecha);
        }

        private string Normalizar(string valor)
        {
            return valor == null ? "" : valor.Trim();
        }

        private RespuestaProveedor Ok()
        {
            return new RespuestaProveedor { Resultado = true, Mensaje = MSG_OK };
        }

        private RespuestaProveedor Error(string mensaje)
        {
            return new RespuestaProveedor { Resultado = false, Mensaje = mensaje };
        }

        private string CifrarAES(string textoPlano)
        {
            byte[] key = ObtenerLlaveAES();

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] textoBytes = Encoding.UTF8.GetBytes(textoPlano ?? "");

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cifrado = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);
                    byte[] resultado = new byte[aes.IV.Length + cifrado.Length];

                    Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
                    Buffer.BlockCopy(cifrado, 0, resultado, aes.IV.Length, cifrado.Length);

                    return Convert.ToBase64String(resultado);
                }
            }
        }

        private string DescifrarAES(string textoEncriptado)
        {
            byte[] data = Convert.FromBase64String(textoEncriptado);

            if (data.Length <= 16)
            {
                throw new CryptographicException("Dato cifrado inválido.");
            }

            byte[] key = ObtenerLlaveAES();

            byte[] iv = new byte[16];
            byte[] cifrado = new byte[data.Length - 16];

            Buffer.BlockCopy(data, 0, iv, 0, 16);
            Buffer.BlockCopy(data, 16, cifrado, 0, cifrado.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] plano = decryptor.TransformFinalBlock(cifrado, 0, cifrado.Length);
                    return Encoding.UTF8.GetString(plano);
                }
            }
        }

        private string CadenaConexionProveedor
        {
            get
            {
                ConnectionStringSettings configuracion =
                    ConfigurationManager.ConnectionStrings["ProveedorTelefonico"];

                return configuracion == null
                    || string.IsNullOrWhiteSpace(configuracion.ConnectionString)
                    ? SQL_CONNECTION_PREDETERMINADA
                    : configuracion.ConnectionString;
            }
        }

        private string HostProveedor
        {
            get
            {
                string valor = ConfigurationManager.AppSettings["ProveedorHost"];
                return string.IsNullOrWhiteSpace(valor) ? HOST_PREDETERMINADO : valor.Trim();
            }
        }

        private int PuertoProveedor
        {
            get
            {
                int valor;
                return int.TryParse(ConfigurationManager.AppSettings["ProveedorPuerto"], out valor)
                    && valor > 0 && valor <= 65535
                    ? valor
                    : PUERTO_PREDETERMINADO;
            }
        }

        private int TimeoutProveedorMs
        {
            get
            {
                int valor;
                return int.TryParse(ConfigurationManager.AppSettings["ProveedorTimeoutMs"], out valor)
                    && valor >= 1000
                    ? valor
                    : TIMEOUT_PREDETERMINADO_MS;
            }
        }

        private byte[] ObtenerLlaveAES()
        {
            string valor = ConfigurationManager.AppSettings["AesKey"];
            string llave = string.IsNullOrEmpty(valor) ? AES_KEY_PREDETERMINADA : valor;
            byte[] bytes = Encoding.UTF8.GetBytes(llave);

            if (bytes.Length != 16 && bytes.Length != 24 && bytes.Length != 32)
            {
                throw new CryptographicException("La llave AES debe tener 16, 24 o 32 bytes.");
            }

            return bytes;
        }
    }

    public class RespuestaProveedor
    {
        public bool Resultado { get; set; }
        public string Mensaje { get; set; }
    }

    public class RespuestaLineasProveedor
    {
        public bool Resultado { get; set; }
        public string Mensaje { get; set; }
        public List<LineaAdministrativa> Lineas { get; set; }
    }

    public class LineaAdministrativa
    {
        public string Telefono { get; set; }
        public string IdentificadorTelefono { get; set; }
        public string IdentificadorTarjeta { get; set; }
        public string TipoServicio { get; set; }
        public string Estado { get; set; }
        public string IdentificacionCliente { get; set; }
    }
}
