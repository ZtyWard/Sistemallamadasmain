using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages;

public class ActivarLineaModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedor;

    public ActivarLineaModel(ProveedorWebServiceClient proveedor) =>
        _proveedor = proveedor;

    public IReadOnlyList<LineaAdministrativaProveedor> Lineas { get; private set; } =
        Array.Empty<LineaAdministrativaProveedor>();

    [BindProperty]
    public string Telefono { get; set; } = string.Empty;

    [BindProperty]
    public string IdentificacionCliente { get; set; } = string.Empty;

    public string Mensaje { get; private set; } = string.Empty;

    public bool EsError { get; private set; }

    public async Task OnGetAsync() => await CargarLineasAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarLineasAsync();

        IdentificacionCliente = IdentificacionCliente?.Trim() ?? string.Empty;

        if (IdentificacionCliente.Length != 9
            || !IdentificacionCliente.All(char.IsDigit))
            return Error("La identificación del cliente debe contener exactamente 9 dígitos.");

        LineaAdministrativaProveedor? linea = Lineas.FirstOrDefault(
            x => x.Telefono.Equals(Telefono?.Trim(), StringComparison.Ordinal));

        if (linea is null)
            return Error("Debe seleccionar una línea disponible.");

        try
        {
            string respuesta = await _proveedor.ActivarLineaAsync(
                linea.Telefono,
                linea.IdentificadorTelefono,
                linea.IdentificadorTarjeta,
                linea.TipoServicio,
                IdentificacionCliente);

            if (!respuesta.Equals("OK", StringComparison.OrdinalIgnoreCase))
                return Error("Error al realizar el proceso");

            Telefono = string.Empty;
            IdentificacionCliente = string.Empty;
            Mensaje = "¡Proceso finalizado de forma exitosa!";
            EsError = false;
            await CargarLineasAsync();
        }
        catch
        {
            return Error("Error al realizar el proceso");
        }

        return Page();
    }

    private async Task CargarLineasAsync()
    {
        try
        {
            ResultadoListadoLineasWeb resultado =
                await _proveedor.ListarLineasDisponiblesAsync();
            Lineas = resultado.Lineas;

            if (!resultado.Exitoso && string.IsNullOrWhiteSpace(Mensaje))
            {
                Mensaje = "No fue posible cargar las líneas disponibles.";
                EsError = true;
            }
        }
        catch
        {
            Lineas = Array.Empty<LineaAdministrativaProveedor>();

            if (string.IsNullOrWhiteSpace(Mensaje))
            {
                Mensaje = "No fue posible cargar las líneas disponibles.";
                EsError = true;
            }
        }
    }

    private PageResult Error(string mensaje)
    {
        Mensaje = mensaje;
        EsError = true;
        return Page();
    }
}
