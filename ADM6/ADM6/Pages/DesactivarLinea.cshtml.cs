using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages;

public class DesactivarLineaModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedor;
    private readonly AutenticacionWebServiceClient _autenticacion;

    public DesactivarLineaModel(
        ProveedorWebServiceClient proveedor,
        AutenticacionWebServiceClient autenticacion)
    {
        _proveedor = proveedor;
        _autenticacion = autenticacion;
    }

    public IReadOnlyList<LineaActivaAdministrativa> Lineas { get; private set; } =
        Array.Empty<LineaActivaAdministrativa>();

    [TempData]
    public string Mensaje { get; set; } = string.Empty;

    [TempData]
    public bool EsError { get; set; }

    public async Task OnGetAsync() => await CargarLineasAsync();

    public async Task<IActionResult> OnPostAsync(string telefono)
    {
        await CargarLineasAsync();
        LineaActivaAdministrativa? linea = Lineas.FirstOrDefault(
            x => x.Telefono.Equals(telefono?.Trim(), StringComparison.Ordinal));

        if (linea is null)
        {
            Mensaje = "Error al realizar el proceso";
            EsError = true;
            return RedirectToPage();
        }

        try
        {
            string respuesta = await _proveedor.DesactivarLineaAsync(
                linea.Telefono,
                linea.IdentificadorTelefono,
                linea.IdentificadorTarjeta,
                linea.TipoServicio,
                linea.IdentificacionCliente);

            EsError = !respuesta.Equals("OK", StringComparison.OrdinalIgnoreCase);
            Mensaje = EsError
                ? "Error al realizar el proceso"
                : "¡Proceso finalizado de forma exitosa!";
        }
        catch
        {
            Mensaje = "Error al realizar el proceso";
            EsError = true;
        }

        return RedirectToPage();
    }

    private async Task CargarLineasAsync()
    {
        try
        {
            ResultadoListadoLineasWeb resultado =
                await _proveedor.ListarLineasActivasAsync();

            if (!resultado.Exitoso)
            {
                Lineas = Array.Empty<LineaActivaAdministrativa>();
                Mensaje = "No fue posible cargar las líneas en uso.";
                EsError = true;
                return;
            }

            Dictionary<string, string> nombres = await ObtenerNombresClientesAsync();
            Lineas = resultado.Lineas.Select(linea => new LineaActivaAdministrativa(
                linea.Telefono,
                linea.IdentificadorTelefono,
                linea.IdentificadorTarjeta,
                linea.TipoServicio,
                linea.IdentificacionCliente,
                nombres.TryGetValue(linea.IdentificacionCliente, out string? nombre)
                    ? nombre
                    : "Cliente no registrado en autenticación"))
                .ToList();
        }
        catch
        {
            Lineas = Array.Empty<LineaActivaAdministrativa>();
            Mensaje = "No fue posible cargar las líneas en uso.";
            EsError = true;
        }
    }

    private async Task<Dictionary<string, string>> ObtenerNombresClientesAsync()
    {
        try
        {
            var usuarios = new List<UsuarioListadoWeb>();
            usuarios.AddRange(await _autenticacion.ListarUsuariosAsync(1));
            usuarios.AddRange(await _autenticacion.ListarUsuariosAsync(2));
            return usuarios
                .GroupBy(x => x.Identificacion)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => NombreCompleto(grupo.First()),
                    StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static string NombreCompleto(UsuarioListadoWeb usuario) =>
        string.Join(" ", new[]
        {
            usuario.Nombre,
            usuario.PrimerApellido,
            usuario.SegundoApellido
        }.Where(valor => !string.IsNullOrWhiteSpace(valor)));
}

public sealed record LineaActivaAdministrativa(
    string Telefono,
    string IdentificadorTelefono,
    string IdentificadorTarjeta,
    string TipoServicio,
    string IdentificacionCliente,
    string NombreCliente);
