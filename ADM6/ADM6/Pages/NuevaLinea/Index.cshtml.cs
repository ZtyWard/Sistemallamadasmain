using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages.NuevaLinea;

public class IndexModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedor;

    public IndexModel(ProveedorWebServiceClient proveedor) =>
        _proveedor = proveedor;

    public IReadOnlyList<LineaAdministrativaProveedor> Lineas { get; private set; } =
        Array.Empty<LineaAdministrativaProveedor>();

    [TempData]
    public string Mensaje { get; set; } = string.Empty;

    [TempData]
    public bool EsError { get; set; }

    public async Task OnGetAsync() => await CargarLineasAsync();

    public async Task<IActionResult> OnPostEliminarAsync(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)
            || telefono.Length != 8
            || !telefono.All(char.IsDigit))
        {
            Mensaje = "Error al realizar el proceso";
            EsError = true;
            return RedirectToPage();
        }

        try
        {
            string respuesta = await _proveedor
                .EliminarLineaDisponibleAsync(telefono.Trim());
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
                await _proveedor.ListarLineasDisponiblesAsync();
            Lineas = resultado.Lineas;

            if (!resultado.Exitoso)
            {
                Mensaje = "No fue posible cargar las líneas disponibles.";
                EsError = true;
            }
        }
        catch
        {
            Lineas = Array.Empty<LineaAdministrativaProveedor>();
            Mensaje = "No fue posible cargar las líneas disponibles.";
            EsError = true;
        }
    }
}
