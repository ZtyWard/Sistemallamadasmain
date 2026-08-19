using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages.Cliente.RecargarSaldo;

public class IndexModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedorClient;

    public IndexModel(ProveedorWebServiceClient proveedorClient)
    {
        _proveedorClient = proveedorClient;
    }


    public List<LineaPrepago> Lineas { get; set; } =
        new List<LineaPrepago>();


    [BindProperty]
    public string NumeroLinea { get; set; } =
        "";


    [BindProperty]
    public decimal Monto { get; set; }


    public decimal SaldoActual
    {
        get
        {
            var linea = Lineas.FirstOrDefault(
                x => x.Numero == NumeroLinea);

            return linea?.Saldo ?? 0;
        }
    }


    public string Mensaje { get; set; } = "";


    public bool EsError { get; set; }


    // ==========================================================
    // GET
    // ==========================================================

    public async Task<IActionResult> OnGetAsync(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(
            HttpContext.Session.GetString("ClienteIdentificacion")))
            return RedirectToPage("/Cliente/Login");

        NumeroLinea = telefono?.Trim() ?? "";
        await CargarLineasAsync();
        return Page();
    }


    // ==========================================================
    // POST - RECARGAR SALDO
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
        // Validar línea seleccionada
        // ------------------------------------------------------

        if (string.IsNullOrWhiteSpace(NumeroLinea))
        {
            EsError = true;

            Mensaje =
                "Debe seleccionar una línea.";

            return Page();
        }


        // ------------------------------------------------------
        // Validar monto
        // ------------------------------------------------------

        if (Monto <= 0)
        {
            EsError = true;

            Mensaje =
                "El monto de la recarga debe ser mayor que cero.";

            return Page();
        }


        // CLIENTE5 solicita un monto entero.
        if (Monto != decimal.Truncate(Monto))
        {
            EsError = true;

            Mensaje =
                "El monto de la recarga debe ser un número entero.";

            return Page();
        }


        // ------------------------------------------------------
        // Buscar línea
        // ------------------------------------------------------

        var linea = Lineas.FirstOrDefault(
            x => x.Numero == NumeroLinea);


        if (linea == null)
        {
            EsError = true;

            Mensaje =
                "La línea seleccionada no existe.";

            return Page();
        }


        // ------------------------------------------------------
        // CONEXIÓN REAL CON EL PROVEEDOR JAVA
        // ------------------------------------------------------

        try
        {
            string respuesta =
                await _proveedorClient.RecargarSaldoAsync(
                    NumeroLinea,
                    Monto);


            // --------------------------------------------------
            // El proveedor confirmó la operación
            // --------------------------------------------------

            if (string.Equals(
                    respuesta,
                    "OK",
                    StringComparison.OrdinalIgnoreCase))
            {
                /*
                 * Actualizamos el valor mostrado en la UI
                 * para reflejar inmediatamente la recarga.
                 *
                 * El saldo real ya fue actualizado por Java
                 * mediante TelefonoDAO.actualizarSaldo().
                 */

                decimal montoAplicado = Monto;

                await CargarLineasAsync();

                Mensaje =
                    $"Recarga de ₡{montoAplicado:N0} realizada correctamente.";


                EsError = false;


                Monto = 0;


                return Page();
            }


            // --------------------------------------------------
            // El proveedor rechazó la operación
            // --------------------------------------------------

            EsError = true;

            Mensaje =
                ObtenerMensajeErrorProveedor(
                    respuesta);

            return Page();
        }
        catch (OperationCanceledException)
        {
            EsError = true;

            Mensaje =
                "El proveedor tardó demasiado en responder.";

            return Page();
        }
        catch (HttpRequestException)
        {
            EsError = true;

            Mensaje =
                "No se pudo conectar con el Proveedor.";

            return Page();
        }
        catch (Exception)
        {
            EsError = true;

            Mensaje =
                "No fue posible realizar la recarga.";

            return Page();
        }
    }


    // ==========================================================
    // MENSAJES DEL PROVEEDOR
    // ==========================================================

    private string ObtenerMensajeErrorProveedor(
        string respuesta)
    {
        if (string.IsNullOrWhiteSpace(respuesta))
        {
            return "El Proveedor no devolvió una respuesta.";
        }


        return respuesta switch
        {
            "ERROR" =>
                "El Proveedor rechazó la recarga.",

            "Datos Incompletos" =>
                "Los datos de la recarga están incompletos.",

            _ =>
                $"El Proveedor rechazó la recarga: {respuesta}"
        };
    }


    // ==========================================================
    // DATOS REALES DEL PROVEEDOR
    // ==========================================================

    private async Task<bool> CargarLineasAsync()
    {
        try
        {
            var datos =
                await _proveedorClient
                    .ListarLineasPrepagoAsync(
                        HttpContext.Session.GetString("ClienteIdentificacion") ?? "");

            Lineas = datos
                .Select(x => new LineaPrepago
                {
                    Numero = x.Telefono,
                    Saldo = x.Saldo
                })
                .ToList();

            if (Lineas.Count == 0)
            {
                EsError = true;
                Mensaje =
                    "No existen líneas PREPAGO activas para recargar.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NumeroLinea)
                || !Lineas.Any(x => x.Numero == NumeroLinea))
            {
                NumeroLinea = Lineas.First().Numero;
            }

            return true;
        }
        catch (Exception)
        {
            Lineas = new List<LineaPrepago>();
            EsError = true;
            Mensaje =
                "No se pudieron consultar las líneas del Proveedor.";
            return false;
        }
    }


    // ==========================================================
    // MODELO DE LÍNEA PREPAGO
    // ==========================================================

    public class LineaPrepago
    {
        public string Numero { get; set; } = "";

        public decimal Saldo { get; set; }
    }
}
