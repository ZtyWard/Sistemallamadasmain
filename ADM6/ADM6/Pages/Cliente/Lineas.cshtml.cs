using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages.Cliente;

public class LineasModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedor;

    public LineasModel(ProveedorWebServiceClient proveedor) =>
        _proveedor = proveedor;

    public List<LineaClienteProveedor> Prepago { get; set; } = new();
    public List<LineaClienteProveedor> Postpago { get; set; } = new();
    public string Mensaje { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        string? identificacion =
            HttpContext.Session.GetString("ClienteIdentificacion");

        if (string.IsNullOrWhiteSpace(identificacion))
            return RedirectToPage("/Cliente/Login");

        try
        {
            List<LineaClienteProveedor> lineas =
                await _proveedor.ListarLineasClienteAsync(identificacion);
            Prepago = lineas.Where(x =>
                x.TipoServicio.Equals("PREPAGO", StringComparison.OrdinalIgnoreCase)).ToList();
            Postpago = lineas.Where(x =>
                x.TipoServicio.Equals("POSTPAGO", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        catch
        {
            Mensaje = "No fue posible consultar las líneas mediante el Web Service del Proveedor.";
        }

        return Page();
    }
}
