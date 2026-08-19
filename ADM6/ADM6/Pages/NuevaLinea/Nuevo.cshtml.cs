using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages.NuevaLinea;

public class NuevoModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedor;

    public NuevoModel(ProveedorWebServiceClient proveedor) =>
        _proveedor = proveedor;

    [BindProperty]
    public string Telefono { get; set; } = string.Empty;

    [BindProperty]
    public string IdentificadorTelefono { get; set; } = string.Empty;

    [BindProperty]
    public string IdentificadorTarjeta { get; set; } = string.Empty;

    [BindProperty]
    public string TipoServicio { get; set; } = "PREPAGO";

    public string Mensaje { get; private set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        Telefono = Telefono?.Trim() ?? string.Empty;
        IdentificadorTelefono = IdentificadorTelefono?.Trim() ?? string.Empty;
        IdentificadorTarjeta = IdentificadorTarjeta?.Trim() ?? string.Empty;
        TipoServicio = TipoServicio?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!EsNumericoExacto(Telefono, 8))
            return Error("El número de teléfono debe contener exactamente 8 dígitos.");

        if (!EsNumericoExacto(IdentificadorTelefono, 16))
            return Error("El identificador del teléfono debe contener exactamente 16 dígitos.");

        if (!EsNumericoExacto(IdentificadorTarjeta, 19))
            return Error("El identificador de tarjeta debe contener exactamente 19 dígitos.");

        if (TipoServicio is not ("PREPAGO" or "POSTPAGO"))
            return Error("Debe seleccionar un tipo de servicio válido.");

        try
        {
            string respuesta = await _proveedor.RegistrarLineaAsync(
                Telefono, IdentificadorTelefono, IdentificadorTarjeta, TipoServicio);

            if (!respuesta.Equals("OK", StringComparison.OrdinalIgnoreCase))
                return Error("Error al realizar el proceso");

            TempData["Mensaje"] = "¡Proceso finalizado de forma exitosa!";
            TempData["EsError"] = false;
            return RedirectToPage("/NuevaLinea/Index");
        }
        catch
        {
            return Error("Error al realizar el proceso");
        }
    }

    private PageResult Error(string mensaje)
    {
        Mensaje = mensaje;
        return Page();
    }

    private static bool EsNumericoExacto(string valor, int longitud) =>
        valor.Length == longitud && valor.All(char.IsDigit);
}
