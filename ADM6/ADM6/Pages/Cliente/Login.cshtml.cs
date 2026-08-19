using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ADM6.Pages.Cliente;

public class LoginModel : PageModel
{
    private readonly AutenticacionWebServiceClient _autenticacion;

    public LoginModel(AutenticacionWebServiceClient autenticacion) =>
        _autenticacion = autenticacion;

    [BindProperty, Required]
    public string Usuario { get; set; } = "";

    [BindProperty, Required]
    public string Contrasena { get; set; } = "";

    public string Mensaje { get; set; } = "";

    public void OnGet()
    {
        HttpContext.Session.Clear();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Mensaje = "Debe ingresar usuario y contraseña.";
            return Page();
        }

        try
        {
            ResultadoAutenticacionWeb resultado =
                await _autenticacion.AutenticarAsync(
                    Usuario.Trim(), Contrasena, "cliente");

            if (!resultado.Exitoso || resultado.Tipo != 2)
            {
                Mensaje = "Usuario y/o contraseña incorrectos";
                return Page();
            }

            HttpContext.Session.SetString(
                "ClienteIdentificacion", resultado.Identificacion);
            HttpContext.Session.SetString(
                "ClienteNombre", resultado.Nombre);
            HttpContext.Session.SetInt32("ClienteTipo", resultado.Tipo);

            return RedirectToPage("/Cliente/Lineas");
        }
        catch
        {
            Mensaje = "No fue posible comunicarse con el servicio de autenticación.";
            return Page();
        }
    }
}
