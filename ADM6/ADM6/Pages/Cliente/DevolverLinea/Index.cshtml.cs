using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADM6.Pages.Cliente.DevolverLinea;

public class IndexModel : PageModel
{
    private readonly ProveedorWebServiceClient _proveedorClient;


    public IndexModel(
        ProveedorWebServiceClient proveedorClient)
    {
        _proveedorClient =
            proveedorClient;
    }


    // ==========================================================
    // DATOS
    // ==========================================================

    [BindProperty]
    public string Telefono { get; set; } = "";


    [BindProperty]
    public bool ConfirmarDevolucion { get; set; }

    public List<LineaClienteProveedor> Lineas { get; set; } = new();


    // ==========================================================
    // MENSAJES
    // ==========================================================

    public string Mensaje { get; set; } = "";


    public bool EsError { get; set; }


    // ==========================================================
    // GET
    // ==========================================================

    public async Task<IActionResult> OnGetAsync(string? telefono)
    {
        if (!await CargarLineasAsync())
            return string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ClienteIdentificacion"))
                ? RedirectToPage("/Cliente/Login")
                : Page();

        Telefono = telefono?.Trim() ?? Lineas.FirstOrDefault()?.Telefono ?? "";
        ConfirmarDevolucion = false;
        return Page();
    }


    // ==========================================================
    // POST
    // ==========================================================

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await CargarLineasAsync())
            return string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("ClienteIdentificacion"))
                ? RedirectToPage("/Cliente/Login")
                : Page();

        Telefono =
            Telefono?.Trim() ?? "";


        // ------------------------------------------------------
        // VALIDAR TELÉFONO
        // ------------------------------------------------------

        if (!TelefonoValido(Telefono))
        {
            EsError = true;

            Mensaje =
                "El número de teléfono debe contener exactamente 8 dígitos.";

            return Page();
        }

        if (!Lineas.Any(x => x.Telefono == Telefono))
        {
            EsError = true;
            Mensaje = "La línea seleccionada no pertenece al cliente conectado.";
            return Page();
        }


        // ------------------------------------------------------
        // VALIDAR CONFIRMACIÓN
        // ------------------------------------------------------

        if (!ConfirmarDevolucion)
        {
            EsError = true;

            Mensaje =
                "Debe confirmar que desea devolver la línea.";

            return Page();
        }


        try
        {
            // --------------------------------------------------
            // LLAMADA REAL AL PROVEEDOR
            //
            // CLIENTE7|DEVOLVER|telefono
            // --------------------------------------------------

            string respuesta =
                await _proveedorClient.DevolverLineaAsync(
                    Telefono);


            // --------------------------------------------------
            // DEVOLUCIÓN EXITOSA
            // --------------------------------------------------

            if (respuesta.Equals(
                    "OK",
                    StringComparison.OrdinalIgnoreCase))
            {
                EsError = false;

                Mensaje =
                    "Registro exitoso. La línea fue devuelta correctamente.";

                Telefono = "";

                ConfirmarDevolucion = false;

                return Page();
            }


            // --------------------------------------------------
            // LÍNEA NO CORRESPONDE
            // --------------------------------------------------

            if (respuesta.Equals(
                    "Telefono no corresponde",
                    StringComparison.OrdinalIgnoreCase))
            {
                EsError = true;

                Mensaje =
                    "La línea no está activa, no corresponde al cliente o no existe.";

                return Page();
            }


            // --------------------------------------------------
            // DATOS INCOMPLETOS
            // --------------------------------------------------

            if (respuesta.Equals(
                    "Datos Incompletos",
                    StringComparison.OrdinalIgnoreCase))
            {
                EsError = true;

                Mensaje =
                    "Los datos de la línea están incompletos o son incorrectos.";

                return Page();
            }


            // --------------------------------------------------
            // FALLA DE ACTIVACIÓN / DESACTIVACIÓN
            // --------------------------------------------------

            if (respuesta.Equals(
                    "Activación fallida",
                    StringComparison.OrdinalIgnoreCase))
            {
                EsError = true;

                Mensaje =
                    "No fue posible completar la devolución de la línea.";

                return Page();
            }

            if (respuesta.Equals(
                    "Factura pendiente",
                    StringComparison.OrdinalIgnoreCase))
            {
                EsError = true;

                Mensaje =
                    "La línea POSTPAGO tiene facturas pendientes y no puede devolverse.";

                return Page();
            }


            // --------------------------------------------------
            // ERROR GENERAL
            // --------------------------------------------------

            EsError = true;

            Mensaje =
                "El Proveedor rechazó la devolución de la línea.";

            return Page();
        }
        catch (OperationCanceledException)
        {
            EsError = true;

            Mensaje =
                "La conexión con el Proveedor tardó demasiado.";

            return Page();
        }
        catch (InvalidOperationException ex)
        {
            EsError = true;

            Mensaje =
                ex.Message;

            return Page();
        }
        catch (Exception)
        {
            EsError = true;

            Mensaje =
                "Ocurrió un error al comunicarse con el Proveedor.";

            return Page();
        }
    }


    // ==========================================================
    // VALIDACIÓN TELÉFONO
    // ==========================================================

    private static bool TelefonoValido(
        string telefono)
    {
        if (string.IsNullOrWhiteSpace(
                telefono))
        {
            return false;
        }


        if (telefono.Length != 8)
        {
            return false;
        }


        foreach (char caracter in telefono)
        {
            if (!char.IsDigit(
                    caracter))
            {
                return false;
            }
        }


        return true;
    }

    private async Task<bool> CargarLineasAsync()
    {
        string identificacion =
            HttpContext.Session.GetString("ClienteIdentificacion") ?? "";
        if (string.IsNullOrWhiteSpace(identificacion))
            return false;

        try
        {
            Lineas = await _proveedorClient
                .ListarLineasClienteAsync(identificacion);
            return true;
        }
        catch
        {
            EsError = true;
            Mensaje = "No fue posible consultar las líneas mediante el Web Service.";
            return false;
        }
    }
}
