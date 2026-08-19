using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages;

public class FacturacionModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedorClient;

    public ADM6.Models.FacturacionViewModel Facturacion { get; set; } = new();

    public FacturacionModel(ProveedorWebServiceClient proveedorClient) =>
        _proveedorClient = proveedorClient;

    public async Task OnGetAsync() => await CargarUltimaFacturacionAsync();

    public async Task<IActionResult> OnPostCalcularAsync(
        DateTime fechaInicio, DateTime fechaFin)
    {
        Facturacion.FechaInicio = fechaInicio;
        Facturacion.FechaFin = fechaFin;

        if (fechaFin < fechaInicio)
        {
            Facturacion.EsError = true;
            Facturacion.Mensaje = "La fecha máxima de pago no puede ser anterior a la fecha de cálculo.";
            return Page();
        }

        try
        {
            string respuesta = await _proveedorClient
                .CalcularFacturacionAsync(fechaInicio, fechaFin);

            if (respuesta.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                Facturacion.FacturacionRealizada = true;
                Facturacion.Mensaje = "Facturación calculada correctamente.";
                await CargarUltimaFacturacionAsync();
            }
            else
            {
                Facturacion.EsError = true;
                Facturacion.Mensaje = "El proveedor devolvió: " + respuesta;
            }
        }
        catch (Exception ex)
        {
            Facturacion.EsError = true;
            Facturacion.Mensaje = "No fue posible comunicarse con el Web Service del Proveedor: " + ex.Message;
        }

        return Page();
    }

    private async Task CargarUltimaFacturacionAsync()
    {
        try
        {
            string respuesta = await _proveedorClient.ObtenerUltimaFechaFacturacionAsync();
            if (DateTime.TryParse(respuesta, out DateTime fecha))
                Facturacion.UltimaFacturacion = fecha;
        }
        catch
        {
            // La página puede abrir aunque todavía no exista facturación previa.
        }
    }
}
