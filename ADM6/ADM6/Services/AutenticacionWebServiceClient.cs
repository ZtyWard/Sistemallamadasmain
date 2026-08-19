using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace ADM6.Services;

public sealed class AutenticacionWebServiceClient
{
    private const string NsAuth1 = "http://centralgeneral/autenticacion";
    private const string NsAuth2 = "http://centralgeneral/autenticacion2";
    private const string Clave = "12345678901234567890123456789012";
    private readonly HttpClient _http;
    private readonly string _urlAuth1;
    private readonly string _urlAuth2;

    public AutenticacionWebServiceClient(
        IHttpClientFactory factory,
        IConfiguration configuration)
    {
        _http = factory.CreateClient("soap");
        _urlAuth1 = configuration["Services:Autenticacion1Url"]
            ?? "http://localhost:8114/WS_AUTENTICACION1.asmx";
        _urlAuth2 = configuration["Services:Autenticacion2Url"]
            ?? "http://localhost:8112/WS_AUTENTICACION2.asmx";
    }

    public async Task<ResultadoAutenticacionWeb> AutenticarAsync(
        string usuario, string contrasena, string tipo)
    {
        XDocument xml = await InvocarAsync(
            _urlAuth1, NsAuth1, "Autenticar", new()
            {
                ["usuarioEncriptado"] = Cifrar(usuario),
                ["contrasenaEncriptada"] = Cifrar(contrasena),
                ["tipoUsuario"] = tipo
            });

        return new ResultadoAutenticacionWeb(
            Booleano(xml, "Resultado"),
            Valor(xml, "Mensaje"),
            Valor(xml, "Identificacion"),
            Valor(xml, "Nombre"),
            int.TryParse(Valor(xml, "Tipo"), out int numero) ? numero : 0);
    }

    public async Task<ResultadoOperacionWeb> CrearUsuarioAsync(
        UsuarioEdicionWeb usuario, int tipo)
    {
        XDocument xml = await InvocarAsync(
            _urlAuth2, NsAuth2, "CrearUsuario", ParametrosUsuario(usuario, tipo));
        return Resultado(xml);
    }

    public async Task<ResultadoOperacionWeb> ModificarUsuarioAsync(
        UsuarioEdicionWeb usuario)
    {
        var parametros = ParametrosUsuario(usuario, 0);
        parametros.Remove("estado");
        parametros.Remove("tipo");
        XDocument xml = await InvocarAsync(
            _urlAuth2, NsAuth2, "ModificarUsuario", parametros);
        return Resultado(xml);
    }

    public async Task<ResultadoOperacionWeb> CambiarEstadoAsync(
        string identificacion, string estado)
    {
        XDocument xml = await InvocarAsync(
            _urlAuth2, NsAuth2, "CambiarEstadoUsuario", new()
            {
                ["identificacion"] = identificacion,
                ["estado"] = estado
            });
        return Resultado(xml);
    }

    public async Task<ResultadoOperacionWeb> EliminarUsuarioAsync(
        string identificacion)
    {
        XDocument xml = await InvocarAsync(
            _urlAuth2, NsAuth2, "EliminarUsuario", new()
            {
                ["identificacion"] = identificacion
            });
        return Resultado(xml);
    }

    public async Task<List<UsuarioListadoWeb>> ListarUsuariosAsync(int tipo)
    {
        XDocument xml = await InvocarAsync(
            _urlAuth2, NsAuth2, "ListarUsuarios", new()
            {
                ["tipo"] = tipo.ToString()
            });

        return xml.Descendants()
            .Where(x => x.Name.LocalName == "UsuarioConsulta")
            .Select(x => new UsuarioListadoWeb(
                Hijo(x, "Identificacion"), Hijo(x, "Nombre"),
                Hijo(x, "PrimerApellido"), Hijo(x, "SegundoApellido"),
                Hijo(x, "CorreoElectronico"), Hijo(x, "Usuario"),
                Hijo(x, "Contrasena"), Hijo(x, "Estado"),
                int.TryParse(Hijo(x, "Tipo"), out int valor) ? valor : 0))
            .ToList();
    }

    private static Dictionary<string, string> ParametrosUsuario(
        UsuarioEdicionWeb usuario, int tipo) => new()
    {
        ["identificacion"] = usuario.Identificacion,
        ["nombre"] = usuario.Nombre,
        ["primerApellido"] = usuario.PrimerApellido,
        ["segundoApellido"] = usuario.SegundoApellido,
        ["correoElectronico"] = usuario.CorreoElectronico,
        ["usuarioEncriptado"] = Cifrar(usuario.Usuario),
        ["contrasenaEncriptada"] = Cifrar(usuario.Contrasena),
        ["estado"] = "activo",
        ["tipo"] = tipo.ToString()
    };

    private async Task<XDocument> InvocarAsync(
        string url, string espacio, string operacion,
        Dictionary<string, string> parametros)
    {
        XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace servicio = espacio;
        var llamada = new XElement(servicio + operacion,
            parametros.Select(p => new XElement(servicio + p.Key, p.Value)));
        var sobre = new XDocument(new XElement(soap + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soap", soap),
            new XElement(soap + "Body", llamada)));

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("SOAPAction", $"\"{espacio}/{operacion}\"");
        request.Content = new StringContent(
            sobre.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        using HttpResponseMessage response = await _http.SendAsync(request);
        string contenido = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        return XDocument.Parse(contenido);
    }

    private static ResultadoOperacionWeb Resultado(XDocument xml) =>
        new(Booleano(xml, "Resultado"), Valor(xml, "Mensaje"));

    private static bool Booleano(XDocument xml, string nombre) =>
        bool.TryParse(Valor(xml, nombre), out bool valor) && valor;

    private static string Valor(XDocument xml, string nombre) =>
        xml.Descendants().FirstOrDefault(x => x.Name.LocalName == nombre)?.Value ?? "";

    private static string Hijo(XElement elemento, string nombre) =>
        elemento.Elements().FirstOrDefault(x => x.Name.LocalName == nombre)?.Value ?? "";

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

public sealed record ResultadoOperacionWeb(bool Exitoso, string Mensaje);
public sealed record ResultadoAutenticacionWeb(
    bool Exitoso, string Mensaje, string Identificacion, string Nombre, int Tipo);
public sealed record UsuarioEdicionWeb(
    string Identificacion, string Nombre, string PrimerApellido,
    string SegundoApellido, string CorreoElectronico,
    string Usuario, string Contrasena);
public sealed record UsuarioListadoWeb(
    string Identificacion, string Nombre, string PrimerApellido,
    string SegundoApellido, string CorreoElectronico,
    string Usuario, string Contrasena, string Estado, int Tipo);
