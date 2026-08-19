using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace ADM6.Services;

public sealed class ProveedorWebServiceClient
{
    private const string Ns = "http://centralgeneral/proveedor";
    private const string Clave = "12345678901234567890123456789012";
    private readonly HttpClient _http;
    private readonly string _url;

    public ProveedorWebServiceClient(
        IHttpClientFactory factory,
        IConfiguration configuration)
    {
        _http = factory.CreateClient("soap");
        _url = configuration["Services:ProveedorUrl"]
            ?? "http://localhost:8111/WS_PROVEEDOR.asmx";
    }

    public async Task<string> ObtenerUltimaFechaFacturacionAsync() =>
        Texto(await InvocarAsync("ObtenerUltimaFechaFacturacion", new()),
            "ObtenerUltimaFechaFacturacion");

    public async Task<string> CalcularFacturacionAsync(DateTime calculo, DateTime maxima) =>
        Respuesta(await InvocarAsync("CalcularFacturacion", new()
        {
            ["fechaCalculo"] = calculo.ToString("yyyy-MM-dd"),
            ["fechaMaximaPago"] = maxima.ToString("yyyy-MM-dd")
        }));

    public async Task<string> RegistrarLineaAsync(
        string telefono, string idTelefono, string idTarjeta, string tipo) =>
        Respuesta(await InvocarAsync("IngresarServicioNuevoDisponible", new()
        {
            ["telefonoEncriptado"] = Cifrar(telefono),
            ["identificadorTelefonoEncriptado"] = Cifrar(idTelefono),
            ["identificadorTarjetaEncriptado"] = Cifrar(idTarjeta),
            ["tipoEncriptado"] = Cifrar(tipo.ToUpperInvariant()),
            ["estadoEncriptado"] = Cifrar("DISPONIBLE")
        }));

    public async Task<string> ActivarLineaAsync(
        string telefono, string idTelefono, string idTarjeta,
        string tipo, string identificacion) =>
        Respuesta(await InvocarAsync("ActivarDesactivarServicio", new()
        {
            ["telefonoEncriptado"] = Cifrar(telefono),
            ["identificadorTelefonoEncriptado"] = Cifrar(idTelefono),
            ["identificadorTarjetaEncriptado"] = Cifrar(idTarjeta),
            ["tipoEncriptado"] = Cifrar(tipo.ToUpperInvariant()),
            ["identificacionClienteEncriptada"] = Cifrar(identificacion),
            ["estadoEncriptado"] = Cifrar("ACTIVO")
        }));

    public async Task<string> DesactivarLineaAsync(
        string telefono, string idTelefono, string idTarjeta,
        string tipo, string identificacion) =>
        Respuesta(await InvocarAsync("ActivarDesactivarServicio", new()
        {
            ["telefonoEncriptado"] = Cifrar(telefono),
            ["identificadorTelefonoEncriptado"] = Cifrar(idTelefono),
            ["identificadorTarjetaEncriptado"] = Cifrar(idTarjeta),
            ["tipoEncriptado"] = Cifrar(tipo.ToUpperInvariant()),
            ["identificacionClienteEncriptada"] = Cifrar(identificacion),
            ["estadoEncriptado"] = Cifrar("INACTIVO")
        }));

    public Task<ResultadoListadoLineasWeb> ListarLineasDisponiblesAsync() =>
        ListarLineasAdministrativasAsync("ListarLineasDisponibles");

    public Task<ResultadoListadoLineasWeb> ListarLineasActivasAsync() =>
        ListarLineasAdministrativasAsync("ListarLineasActivas");

    public async Task<string> EliminarLineaDisponibleAsync(string telefono) =>
        Respuesta(await InvocarAsync("EliminarLineaDisponible", new()
        {
            ["telefono"] = telefono
        }));

    public async Task<string> RecargarSaldoAsync(string telefono, decimal monto) =>
        Respuesta(await InvocarAsync("RecargarSaldo", new()
        {
            ["telefono"] = telefono,
            ["monto"] = monto.ToString(CultureInfo.InvariantCulture)
        }));

    public async Task<string> MarcarFacturaPagadaAsync(int facturaId) =>
        Respuesta(await InvocarAsync("PagarFactura", new()
        {
            ["facturaId"] = facturaId.ToString(CultureInfo.InvariantCulture)
        }));

    public async Task<string> DevolverLineaAsync(string telefono) =>
        Respuesta(await InvocarAsync("DevolverLinea", new()
        {
            ["telefono"] = telefono
        }));

    public async Task<List<LineaClienteProveedor>>
        ListarLineasClienteAsync(string identificacion)
    {
        string valor = await ListadoAsync("ListarLineasCliente", identificacion);
        var resultado = new List<LineaClienteProveedor>();
        foreach (string registro in Registros(valor))
        {
            string[] c = registro.Split(',');
            if (c.Length == 5
                && decimal.TryParse(c[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal saldo)
                && int.TryParse(c[3], out int facturaId)
                && decimal.TryParse(c[4], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto))
            {
                resultado.Add(new(c[0], c[1], saldo, facturaId, monto));
            }
        }
        return resultado;
    }

    public async Task<List<LineaPrepagoProveedor>>
        ListarLineasPrepagoAsync(string identificacion)
    {
        string valor = await ListadoAsync("ListarLineasPrepagoCliente", identificacion);
        var resultado = new List<LineaPrepagoProveedor>();
        foreach (string registro in Registros(valor))
        {
            string[] c = registro.Split(',');
            if (c.Length == 2
                && decimal.TryParse(c[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal saldo))
                resultado.Add(new(c[0], saldo));
        }
        return resultado;
    }

    public async Task<List<FacturaPendienteProveedor>>
        ListarFacturasPendientesAsync(string identificacion)
    {
        string valor = await ListadoAsync("ListarFacturasPendientesCliente", identificacion);
        var resultado = new List<FacturaPendienteProveedor>();
        foreach (string registro in Registros(valor))
        {
            string[] c = registro.Split(',');
            if (c.Length == 4 && int.TryParse(c[0], out int id)
                && decimal.TryParse(c[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto)
                && DateTime.TryParseExact(c[3], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime fecha))
                resultado.Add(new(id, c[1], monto, fecha));
        }
        return resultado;
    }

    private async Task<string> ListadoAsync(string operacion, string identificacion)
    {
        XDocument xml = await InvocarAsync(operacion, new()
        {
            ["identificacionCliente"] = identificacion
        });
        return Texto(xml, operacion);
    }

    private async Task<ResultadoListadoLineasWeb>
        ListarLineasAdministrativasAsync(string operacion)
    {
        XDocument xml = await InvocarAsync(operacion, new());
        XElement? resultado = xml.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == operacion + "Result");

        if (resultado is null)
            return new(false, "Respuesta incorrecta del Web Service", Array.Empty<LineaAdministrativaProveedor>());

        bool exitoso = bool.TryParse(Hijo(resultado, "Resultado"), out bool valor)
            && valor;
        string mensaje = Hijo(resultado, "Mensaje");
        List<LineaAdministrativaProveedor> lineas = resultado.Descendants()
            .Where(x => x.Name.LocalName == "LineaAdministrativa")
            .Select(x => new LineaAdministrativaProveedor(
                Hijo(x, "Telefono"),
                Hijo(x, "IdentificadorTelefono"),
                Hijo(x, "IdentificadorTarjeta"),
                Hijo(x, "TipoServicio"),
                Hijo(x, "Estado"),
                Hijo(x, "IdentificacionCliente")))
            .ToList();

        return new(exitoso, mensaje, lineas);
    }

    private static IEnumerable<string> Registros(string valor)
    {
        string[] partes = valor.Split('|');
        if (partes.Length == 0 || !partes[0].Equals("OK", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("El Web Service rechazó la consulta.");
        return partes.Skip(1);
    }

    private async Task<XDocument> InvocarAsync(
        string operacion, Dictionary<string, string> parametros)
    {
        XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace servicio = Ns;
        var llamada = new XElement(servicio + operacion,
            parametros.Select(p => new XElement(servicio + p.Key, p.Value)));
        var sobre = new XDocument(new XElement(soap + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soap", soap),
            new XElement(soap + "Body", llamada)));

        using var request = new HttpRequestMessage(HttpMethod.Post, _url);
        request.Headers.Add("SOAPAction", $"\"{Ns}/{operacion}\"");
        request.Content = new StringContent(
            sobre.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        using HttpResponseMessage response = await _http.SendAsync(request);
        string contenido = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        return XDocument.Parse(contenido);
    }

    private static string Respuesta(XDocument xml)
    {
        bool ok = bool.TryParse(xml.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Resultado")?.Value, out bool valor) && valor;
        string mensaje = xml.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Mensaje")?.Value ?? "ERROR";
        return ok ? "OK" : mensaje;
    }

    private static string Texto(XDocument xml, string operacion) =>
        xml.Descendants().FirstOrDefault(
            x => x.Name.LocalName == operacion + "Result")?.Value.Trim() ?? "ERROR";

    private static string Hijo(XElement elemento, string nombre) =>
        elemento.Elements().FirstOrDefault(
            x => x.Name.LocalName == nombre)?.Value.Trim() ?? string.Empty;

    private static string Cifrar(string valor)
    {
        byte[] iv = RandomNumberGenerator.GetBytes(16);
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Clave);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        byte[] plano = Encoding.UTF8.GetBytes(valor ?? "");
        byte[] cifrado = aes.CreateEncryptor().TransformFinalBlock(plano, 0, plano.Length);
        return Convert.ToBase64String(iv.Concat(cifrado).ToArray());
    }
}

public sealed record LineaClienteProveedor(
    string Telefono, string TipoServicio, decimal Saldo,
    int FacturaId, decimal MontoPendiente);

public sealed record LineaPrepagoProveedor(string Telefono, decimal Saldo);

public sealed record FacturaPendienteProveedor(
    int Id, string Telefono, decimal Monto, DateTime FechaMaximaPago);

public sealed record LineaAdministrativaProveedor(
    string Telefono,
    string IdentificadorTelefono,
    string IdentificadorTarjeta,
    string TipoServicio,
    string Estado,
    string IdentificacionCliente);

public sealed record ResultadoListadoLineasWeb(
    bool Exitoso,
    string Mensaje,
    IReadOnlyList<LineaAdministrativaProveedor> Lineas);
