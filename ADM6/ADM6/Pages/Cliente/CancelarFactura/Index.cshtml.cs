using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages.Cliente.CancelarFactura;

public class IndexModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedorClient;

    public IndexModel(ProveedorWebServiceClient proveedorClient)
    {
        _proveedorClient = proveedorClient;
    }

    // ==========================================================
    // LINEAS POSTPAGO
    // ==========================================================

    public List<LineaPostpago> Lineas { get; set; } =
        new List<LineaPostpago>();

    [BindProperty]
    public int FacturaId { get; set; }

    public string NumeroLinea =>
        Lineas.FirstOrDefault(
            x => x.FacturaId == FacturaId)
        ?.Numero ?? "";

    // ==========================================================
    // DATOS DE TARJETA
    // ==========================================================

    [BindProperty]
    public string NumeroTarjeta { get; set; } = "";

    [BindProperty]
    public string NombreTarjeta { get; set; } = "";

    [BindProperty]
    public string Vencimiento { get; set; } = "";

    [BindProperty]
    public string CodigoSeguridad { get; set; } = "";

    // ==========================================================
    // MENSAJES
    // ==========================================================

    public string Mensaje { get; set; } = "";

    public bool EsError { get; set; }

    // ==========================================================
    // GET
    // ==========================================================

    public async Task<IActionResult> OnGetAsync(int? facturaId)
    {
        if (string.IsNullOrWhiteSpace(
            HttpContext.Session.GetString("ClienteIdentificacion")))
            return RedirectToPage("/Cliente/Login");

        FacturaId = facturaId ?? 0;
        await CargarLineasAsync();
        return Page();
    }

    // ==========================================================
    // POST - CANCELAR FACTURA
    // ==========================================================

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(
            HttpContext.Session.GetString("ClienteIdentificacion")))
            return RedirectToPage("/Cliente/Login");

        if (!await CargarLineasAsync())
        {
            return Page();
        }

        // ------------------------------------------------------
        // Validar línea
        // ------------------------------------------------------

        if (FacturaId <= 0)
        {
            return Error(
                "Debe seleccionar una línea.");
        }

        LineaPostpago? linea =
            Lineas.FirstOrDefault(
                x => x.FacturaId == FacturaId);

        if (linea == null)
        {
            return Error(
                "La línea seleccionada no existe.");
        }

        // ------------------------------------------------------
        // Validar que exista factura pendiente
        // ------------------------------------------------------

        if (linea.FacturaId <= 0
            || linea.MontoFactura <= 0)
        {
            return Error(
                "La línea seleccionada no tiene una factura pendiente.");
        }

        // ------------------------------------------------------
        // Validar número de tarjeta
        // ------------------------------------------------------

        string numeroTarjeta =
            NormalizarNumeroTarjeta(
                NumeroTarjeta);

        if (numeroTarjeta.Length != 12
            || !numeroTarjeta.All(
                char.IsDigit))
        {
            return Error(
                "El número de tarjeta debe contener 12 dígitos.");
        }

        // ------------------------------------------------------
        // Validar nombre
        // ------------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                NombreTarjeta))
        {
            return Error(
                "Debe ingresar el nombre del dueño de la tarjeta.");
        }

        // ------------------------------------------------------
        // Validar vencimiento
        // ------------------------------------------------------

        if (!VencimientoValido(
                Vencimiento))
        {
            return Error(
                "La fecha de vencimiento no es válida.");
        }

        // ------------------------------------------------------
        // Validar código de seguridad
        // ------------------------------------------------------

        string codigo =
            CodigoSeguridad?.Trim() ?? "";

        if (codigo.Length != 3
            || !codigo.All(
                char.IsDigit))
        {
            return Error(
                "El código de seguridad debe contener 3 dígitos.");
        }

        // ------------------------------------------------------
        // PAGO
        //
        // La historia indica que el pago se considera válido.
        // No almacenamos datos de tarjeta.
        // ------------------------------------------------------

        try
        {
            string respuesta =
                await _proveedorClient.MarcarFacturaPagadaAsync(
                    linea.FacturaId);

            if (string.Equals(
                    respuesta,
                    "OK",
                    StringComparison.OrdinalIgnoreCase))
            {
                linea.Pagada = true;
                linea.MontoFactura = 0;

                await CargarLineasAsync();

                Mensaje =
                    "Registro exitoso";

                EsError = false;

                LimpiarTarjeta();

                return Page();
            }

            return Error(
                "No fue posible cancelar la factura.");
        }
        catch (OperationCanceledException)
        {
            return Error(
                "El proveedor tardó demasiado en responder.");
        }
        catch (HttpRequestException)
        {
            return Error(
                "No se pudo conectar con el Proveedor.");
        }
        catch (Exception)
        {
            return Error(
                "No fue posible cancelar la factura.");
        }
    }

    // ==========================================================
    // VALIDACIÓN DE VENCIMIENTO
    // ==========================================================

    private bool VencimientoValido(
        string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        string limpio =
            valor.Trim();

        if (limpio.Length != 5
            || limpio[2] != '/')
        {
            return false;
        }

        string mesTexto =
            limpio.Substring(0, 2);

        string anioTexto =
            limpio.Substring(3, 2);

        if (!int.TryParse(
                mesTexto,
                out int mes)
            || !int.TryParse(
                anioTexto,
                out int anio))
        {
            return false;
        }

        if (mes < 1 || mes > 12)
        {
            return false;
        }

        int anioCompleto =
            2000 + anio;

        DateTime vencimiento =
            new DateTime(
                anioCompleto,
                mes,
                1);

        DateTime actual =
            new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                1);

        return vencimiento >= actual;
    }

    // ==========================================================
    // NORMALIZAR TARJETA
    // ==========================================================

    private string NormalizarNumeroTarjeta(
        string valor)
    {
        return (valor ?? "")
            .Replace(" ", "")
            .Replace("-", "")
            .Trim();
    }

    // ==========================================================
    // ERROR
    // ==========================================================

    private IActionResult Error(
        string mensaje)
    {
        EsError = true;
        Mensaje = mensaje;

        return Page();
    }

    // ==========================================================
    // LIMPIAR TARJETA
    // ==========================================================

    private void LimpiarTarjeta()
    {
        NumeroTarjeta = "";
        NombreTarjeta = "";
        Vencimiento = "";
        CodigoSeguridad = "";
    }

    // ==========================================================
    // DATOS REALES DEL PROVEEDOR
    // ==========================================================

    private async Task<bool> CargarLineasAsync()
    {
        try
        {
            var facturas =
                await _proveedorClient
                    .ListarFacturasPendientesAsync(
                        HttpContext.Session.GetString("ClienteIdentificacion") ?? "");

            Lineas = facturas
                // Una misma factura debe aparecer una sola vez aunque el
                // proveedor repita accidentalmente el registro.
                .GroupBy(x => x.Id)
                .Select(grupo => grupo.First())
                .Select(x => new LineaPostpago
                {
                    Numero = x.Telefono,
                    FacturaId = x.Id,
                    MontoFactura = x.Monto,
                    Pagada = false
                })
                .OrderBy(x => x.Numero)
                .ThenByDescending(x => x.FacturaId)
                .ToList();

            if (Lineas.Count == 0)
            {
                EsError = true;
                Mensaje =
                    "No existen facturas pendientes.";
                return false;
            }

            if (!Lineas.Any(
                    x => x.FacturaId == FacturaId))
            {
                FacturaId = Lineas.First().FacturaId;
            }

            return true;
        }
        catch (Exception)
        {
            Lineas = new List<LineaPostpago>();
            EsError = true;
            Mensaje =
                "No se pudieron consultar las facturas del Proveedor.";
            return false;
        }
    }

    // ==========================================================
    // MODELO DE LINEA POSTPAGO
    // ==========================================================

    public class LineaPostpago
    {
        public string Numero { get; set; } = "";

        public int FacturaId { get; set; }

        public decimal MontoFactura { get; set; }

        public bool Pagada { get; set; }
    }
}
